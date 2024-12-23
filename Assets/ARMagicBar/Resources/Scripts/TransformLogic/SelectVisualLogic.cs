using System.Collections.Generic;
using System.Linq;
using ARMagicBar.Resources.Scripts.Debugging;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace ARMagicBar.Resources.Scripts.TransformLogic
{
    
    //Sits on each transformable object, handles the "selected material" if clicked on
    [RequireComponent(typeof(TransformableObject))]
    public class SelectVisualLogic : MonoBehaviour
    {
        // [SerializeField] private Transform visual;
        [SerializeField] private Material selectedMaterial;
        [SerializeField] private Material selectedMaterialURP;

        private Material materialToApply;
        
        // private Material[] baseSharedMaterials;
        // private List<(Renderer renderer, Material[] baseMaterial, Material[] selectedMaterial)> rendererToMaterials = new List<(Renderer, Material[], Material[])>();
        // private Material[] selectMaterials;

        [FormerlySerializedAs("objectRenderer")]
        [Tooltip(
            "If the objects renderer is somewhere hidden in the hierarchy, rather drag it in, otherwise it will be automatically added.")]
        [SerializeField]
        private List<Renderer> objectRenderers = new();

        [SerializeField] private UnityEngine.Transform parentOfRenderer;
        
        private Dictionary<Renderer, (Material[], Material[])> rendererBaseSelectMaterialDict = new();
        
        private TransformableObject _transformableObject;

        private void Awake()
        {
            objectRenderers = GetRenderer();

            if (objectRenderers != null)
            {
                SetBaseMaterials();
                SetSelectMaterials();
            }
        }
        
        public List<Renderer> ReturnRenderer()
        {
            return objectRenderers;
        }
        
        //Get all renderer types
        List<Renderer> GetRenderer()
        {
            List<Renderer> childRenderer = new List<Renderer>();
            CustomLog.Instance.InfoLog("Get Renderer");
            if (GetComponentInChildren<PlacementObjectVisual.PlacementObjectVisual>())
            {
                PlacementObjectVisual.PlacementObjectVisual objectVisual = GetComponentInChildren<PlacementObjectVisual.PlacementObjectVisual>();
                parentOfRenderer = objectVisual.transform;
                
                
                if (objectVisual.GetComponent<Renderer>())
                {
                    if (!objectVisual.GetComponent<ParticleSystem>())
                    {
                        Renderer objectRenderer = objectVisual.GetComponent<Renderer>();
                        childRenderer.Add(objectVisual.GetComponent<Renderer>());
                    }
                }
                else
                {
                    UnityEngine.Transform[] childObjects = objectVisual.GetComponentsInChildren<UnityEngine.Transform>();
                    foreach (var childtransf in childObjects)
                    {
                        if(childtransf.GetComponent<Renderer>())
                        {
                            if(childtransf.GetComponent<ParticleSystem>()){ continue; }
                            CustomLog.Instance.InfoLog("Adding " + childtransf.GetComponent<Renderer>().name);
                            childRenderer.Add(childtransf.GetComponent<Renderer>());
                        }
                    }
                }
            }
            return childRenderer;
        }

        //Add base material state
        void SetBaseMaterials()
        {
            foreach (var renderer in objectRenderers)
            {
                Renderer rend = renderer;
                Material[] baseMaterials = renderer.sharedMaterials;
                Material[] selectedMaterials = null;
                
                rendererBaseSelectMaterialDict.Add(rend, (baseMaterials, selectedMaterials));
                
                // (Renderer renderer, Material[] baseMaterials, Material[] selectedMaterials) rendererToMaterial = new(renderer, renderer.sharedMaterials, null);
                // rendererToMaterials.Add(rendererToMaterial);
            }
        }


        
        //Check for URP
        void SetSelectedMaterialDependingOnRP()
        {
            bool isURP = GraphicsSettings.defaultRenderPipeline != null &&
                         GraphicsSettings.defaultRenderPipeline.GetType().Name.Contains("Universal");
            
            if (isURP)
            {
                materialToApply = selectedMaterialURP;
            }
            else
            {
                materialToApply = selectedMaterial;
            }
            
            CustomLog.Instance.InfoLog("Detected Render Pipeline URP? " + isURP);

        }

        //Add selected Material state 
        private void SetSelectMaterials()
        {
            SetSelectedMaterialDependingOnRP();

            for (int i = 0; i < rendererBaseSelectMaterialDict.Count; i++)
            {
                //New Array at the materials length +1
                Material[] selectMaterials = new Material[rendererBaseSelectMaterialDict.ElementAt(i).Value.Item1.Length +1];
                
                for (int j = 0; j < rendererBaseSelectMaterialDict.ElementAt(i).Value.Item1.Length; j++)
                {
                    selectMaterials[j] =  rendererBaseSelectMaterialDict.ElementAt(i).Value.Item1[j];
                }
                
                selectMaterials[selectMaterials.Length - 1] = materialToApply;

                Renderer currentKey = rendererBaseSelectMaterialDict.Keys.ElementAt(i);
                Material[] currentBaseMaterials = rendererBaseSelectMaterialDict.Values.ElementAt(i).Item1;
                
                rendererBaseSelectMaterialDict.Remove(currentKey);
                rendererBaseSelectMaterialDict.Add(currentKey,(currentBaseMaterials, selectMaterials));
            }
        }

        private void Start()
        {
            Hide();
            _transformableObject = GetComponent<TransformableObject>();
            _transformableObject.OnWasSelected += TransformableObjectWasSelected;
            SelectObjectsLogic.Instance.OnDeselectAll += Hide;
        }
        
        void TransformableObjectWasSelected(bool wasSelected)
        {
           CustomLog.Instance.InfoLog("transformable was selected " +  wasSelected +  " " + transform.name);
            if (wasSelected)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void OnDestroy()
        {

            SelectObjectsLogic.Instance.OnDeselectAll -= Hide;
            
            if(_transformableObject)
                _transformableObject.OnWasSelected -= TransformableObjectWasSelected;
        }
        
        void Show()
        {
            for (int i = 0; i < rendererBaseSelectMaterialDict.Count; i++)
            {
                rendererBaseSelectMaterialDict.Keys.ElementAt(i).sharedMaterials =
                    rendererBaseSelectMaterialDict.Values.ElementAt(i).Item2;
            }
        }

        void Hide()
        {
            for (int i = 0; i < rendererBaseSelectMaterialDict.Count; i++)
            {
                rendererBaseSelectMaterialDict.Keys.ElementAt(i).sharedMaterials =
                    rendererBaseSelectMaterialDict.Values.ElementAt(i).Item1;
            }
        }
    }
}