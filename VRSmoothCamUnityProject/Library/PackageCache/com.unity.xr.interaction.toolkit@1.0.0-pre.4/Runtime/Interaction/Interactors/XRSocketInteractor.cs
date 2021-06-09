using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Interactor used for holding interactables via a socket. This component is not designed to be attached to a controller
    /// (thus does not derive from <see cref="XRBaseControllerInteractor"/>) and instead will always attempt to select an interactable that it is
    /// hovering over (though will not perform exclusive selection of that interactable).
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("XR/XR Socket Interactor")]
    [HelpURL(XRHelpURLConstants.k_XRSocketInteractor)]
    public class XRSocketInteractor : XRBaseInteractor
    {
        [SerializeField]
        bool m_ShowInteractableHoverMeshes = true;
        /// <summary>
        /// Whether this socket should show a mesh at socket's attach point for interactables that it is hovering over.
        /// </summary>
        public bool showInteractableHoverMeshes
        {
            get => m_ShowInteractableHoverMeshes;
            set => m_ShowInteractableHoverMeshes = value;
        }

        [SerializeField]
        Material m_InteractableHoverMeshMaterial;
        /// <summary>
        /// Material used for rendering interactable meshes on hover
        /// (a default material will be created if none is supplied).
        /// </summary>
        public Material interactableHoverMeshMaterial
        {
            get => m_InteractableHoverMeshMaterial;
            set => m_InteractableHoverMeshMaterial = value;
        }

        [SerializeField]
        Material m_InteractableCantHoverMeshMaterial;
        /// <summary>
        /// Material used for rendering interactable meshes on hover when there is already a selected object in the socket
        /// (a default material will be created if none is supplied).
        /// </summary>
        public Material interactableCantHoverMeshMaterial
        {
            get => m_InteractableCantHoverMeshMaterial;
            set => m_InteractableCantHoverMeshMaterial = value;
        }

        [SerializeField]
        bool m_SocketActive = true;
        /// <summary>
        /// Whether socket interaction is enabled.
        /// </summary>
        public bool socketActive
        {
            get => m_SocketActive;
            set => m_SocketActive = value;
        }

        [SerializeField]
        float m_InteractableHoverScale = 1f;
        /// <summary>
        /// Scale at which to render hovered interactable.
        /// </summary>
        public float interactableHoverScale
        {
            get => m_InteractableHoverScale;
            set => m_InteractableHoverScale = value;
        }

        [SerializeField]
        float m_RecycleDelayTime = 1f;
        /// <summary>
        /// Sets the amount of time the socket will refuse hovers after an object is removed.
        /// </summary>
        public float recycleDelayTime
        {
            get => m_RecycleDelayTime;
            set => m_RecycleDelayTime = value;
        }

        float m_LastRemoveTime = -100f;

        /// <summary>
        /// Reusable list of valid targets.
        /// </summary>
        readonly List<XRBaseInteractable> m_ValidTargets = new List<XRBaseInteractable>();

        readonly TriggerContactMonitor m_TriggerContactMonitor = new TriggerContactMonitor();

        readonly Dictionary<XRBaseInteractable, ValueTuple<MeshFilter, Renderer>[]> m_MeshFilterCache = new Dictionary<XRBaseInteractable, ValueTuple<MeshFilter, Renderer>[]>();

        /// <summary>
        /// Reusable list of type <see cref="MeshFilter"/> to reduce allocations.
        /// </summary>
        static readonly List<MeshFilter> s_MeshFilters = new List<MeshFilter>();

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            m_TriggerContactMonitor.interactionManager = interactionManager;
            m_TriggerContactMonitor.contactAdded += OnContactAdded;
            m_TriggerContactMonitor.contactRemoved += OnContactRemoved;

            CreateDefaultHoverMaterials();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        /// <param name="other">The other <see cref="Collider"/> involved in this collision.</param>
        protected void OnTriggerEnter(Collider other)
        {
            m_TriggerContactMonitor.AddCollider(other);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        /// <param name="other">The other <see cref="Collider"/> involved in this collision.</param>
        protected void OnTriggerExit(Collider other)
        {
            m_TriggerContactMonitor.RemoveCollider(other);
        }

        /// <summary>
        /// Create the default hover materials
        /// for <see cref="interactableHoverMeshMaterial"/> and <see cref="interactableCantHoverMeshMaterial"/> if necessary.
        /// </summary>
        protected virtual void CreateDefaultHoverMaterials()
        {
            if (m_InteractableHoverMeshMaterial != null && m_InteractableCantHoverMeshMaterial != null)
                return;

            var shaderName = GraphicsSettings.currentRenderPipeline ? "Universal Render Pipeline/Lit" : "Standard";
            var defaultShader = Shader.Find(shaderName);

            if (defaultShader == null)
            {
                Debug.LogWarning("Failed to create default materials for Socket Interactor," +
                    $" was unable to find \"{shaderName}\" Shader. Make sure the shader is included into the game build.", this);
                return;
            }

            if (m_InteractableHoverMeshMaterial == null)
            {
                m_InteractableHoverMeshMaterial = new Material(defaultShader);
                SetMaterialFade(m_InteractableHoverMeshMaterial, new Color(0f, 0f, 1f, 0.6f));
            }

            if (m_InteractableCantHoverMeshMaterial == null)
            {
                m_InteractableCantHoverMeshMaterial = new Material(defaultShader);
                SetMaterialFade(m_InteractableCantHoverMeshMaterial, new Color(1f, 0f, 0f, 0.6f));
            }
        }

        /// <summary>
        /// Set Standard <paramref name="material"/> with Fade rendering mode
        /// and set <paramref name="color"/> as the main color.
        /// </summary>
        /// <param name="material">The <see cref="Material"/> whose properties will be set.</param>
        /// <param name="color">The main color to set.</param>
        static void SetMaterialFade(Material material, Color color)
        {
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetFloat(ShaderPropertyLookup.mode, 2f);
            material.SetInt(ShaderPropertyLookup.srcBlend, (int)BlendMode.SrcAlpha);
            material.SetInt(ShaderPropertyLookup.dstBlend, (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt(ShaderPropertyLookup.zWrite, 0);
            // ReSharper disable StringLiteralTypo
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            // ReSharper restore StringLiteralTypo
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetColor(GraphicsSettings.currentRenderPipeline ? ShaderPropertyLookup.baseColor : ShaderPropertyLookup.color, color);
        }

        /// <inheritdoc />
        protected internal override void OnHoverEntering(HoverEnterEventArgs args)
        {
            base.OnHoverEntering(args);

            s_MeshFilters.Clear();
            args.interactable.GetComponentsInChildren(true, s_MeshFilters);
            if (s_MeshFilters.Count == 0)
                return;

            var interactableTuples = new ValueTuple<MeshFilter, Renderer>[s_MeshFilters.Count];
            for (var i = 0; i < s_MeshFilters.Count; ++i)
            {
                var meshFilter = s_MeshFilters[i];
                interactableTuples[i] = (meshFilter, meshFilter.GetComponent<Renderer>());
            }
            m_MeshFilterCache.Add(args.interactable, interactableTuples);
        }

        /// <inheritdoc />
        protected internal override void OnHoverExiting(HoverExitEventArgs args)
        {
            base.OnHoverExiting(args);
            m_MeshFilterCache.Remove(args.interactable);
        }

        /// <inheritdoc />
        protected internal override void OnSelectExiting(SelectExitEventArgs args)
        {
            base.OnSelectExiting(args);
            m_LastRemoveTime = Time.time;
        }

        /// <inheritdoc />
        public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractor(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic && m_ShowInteractableHoverMeshes && hoverTargets.Count > 0)
                DrawHoveredInteractables();
        }

        Matrix4x4 GetInteractableAttachMatrix(XRGrabInteractable interactable, MeshFilter meshFilter, Vector3 scale)
        {
            var interactableLocalPosition = Vector3.zero;
            var interactableLocalRotation = Quaternion.identity;

            if (interactable.attachTransform != null)
            {
                // localPosition doesn't take into account scaling of parent objects, so scale attachpoint by lossyScale which is the global scale.
                interactableLocalPosition =  Vector3.Scale(interactable.attachTransform.localPosition, interactable.attachTransform.lossyScale);
                interactableLocalRotation = interactable.attachTransform.localRotation;
            }

            var finalPosition = attachTransform.position - interactableLocalPosition;
            var finalRotation = attachTransform.rotation * interactableLocalRotation;

            if(interactable.transform != meshFilter.transform)
            {
                finalPosition += Vector3.Scale(interactable.transform.InverseTransformPoint(meshFilter.transform.position), interactable.transform.lossyScale);
                finalRotation *= Quaternion.Inverse(Quaternion.Inverse(meshFilter.transform.rotation) * interactable.transform.rotation);
            }

            return Matrix4x4.TRS(finalPosition, finalRotation, scale);
        }

        /// <summary>
        /// This method is called automatically in order to draw the interactables that are currently being hovered over.
        /// </summary>
        protected virtual void DrawHoveredInteractables()
        {
            var materialToDrawWith = selectTarget == null ? m_InteractableHoverMeshMaterial : m_InteractableCantHoverMeshMaterial;
            if (materialToDrawWith == null)
                return;

            var mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            var hoveredScale = Mathf.Max(0f, m_InteractableHoverScale);

            foreach (var hoverTarget in hoverTargets)
            {
                var grabTarget = hoverTarget as XRGrabInteractable;
                if (grabTarget == null || grabTarget == selectTarget)
                    continue;

                if (!m_MeshFilterCache.TryGetValue(grabTarget, out var interactableTuples))
                    continue;

                if (interactableTuples == null || interactableTuples.Length == 0)
                    continue;

                foreach (var tuple in interactableTuples)
                {
                    var meshFilter = tuple.Item1;
                    var meshRenderer = tuple.Item2;
                    if (!ShouldDrawHoverMesh(meshFilter, meshRenderer, mainCamera))
                        continue;

                    for (var submeshIndex = 0; submeshIndex < meshFilter.sharedMesh.subMeshCount; ++submeshIndex)
                    {
                        Graphics.DrawMesh(
                            meshFilter.sharedMesh,
                            GetInteractableAttachMatrix(grabTarget, meshFilter, meshFilter.transform.lossyScale * hoveredScale),
                            materialToDrawWith,
                            gameObject.layer, // TODO Why use this Interactor layer instead of the Interactable layer?
                            null, // Draw mesh in all cameras (default value)
                            submeshIndex);
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void GetValidTargets(List<XRBaseInteractable> targets)
        {
            SortingHelpers.SortByDistanceToInteractor(this, m_ValidTargets, targets);
        }

        /// <summary>
        /// (Read Only) Indicates whether this interactor is in a state where it could hover (always <see langword="true"/> for sockets if active).
        /// </summary>
        public override bool isHoverActive => m_SocketActive;

        /// <summary>
        /// (Read Only) Indicates whether this interactor is in a state where it could select (always <see langword="true"/> for sockets if active).
        /// </summary>
        public override bool isSelectActive => m_SocketActive;

        /// <summary>
        /// (Read Only) Indicates whether this interactor requires exclusive selection of an interactable (always <see langword="true"/> for sockets).
        /// </summary>
        public override bool requireSelectExclusive => true;

        /// <inheritdoc />
        public override XRBaseInteractable.MovementType? selectedInteractableMovementTypeOverride => XRBaseInteractable.MovementType.Instantaneous;

        /// <inheritdoc />
        public override bool CanSelect(XRBaseInteractable interactable)
        {
            return base.CanSelect(interactable) && (selectTarget == null || selectTarget == interactable);
        }

        /// <inheritdoc />
        public override bool CanHover(XRBaseInteractable interactable)
        {
            return base.CanHover(interactable) && Time.time > m_LastRemoveTime + m_RecycleDelayTime;
        }

        /// <summary>
        /// This method is called automatically in order to determine whether the Mesh should be drawn.
        /// </summary>
        /// <param name="meshFilter">The <see cref="MeshFilter"/> which will be drawn when returning <see langword="true"/>.</param>
        /// <param name="meshRenderer">The <see cref="Renderer"/> on the same <see cref="GameObject"/> as the <paramref name="meshFilter"/>.</param>
        /// <param name="mainCamera">The Main Camera.</param>
        /// <returns>Returns <see langword="true"/> if the Mesh should be drawn. Otherwise, returns <see langword="false"/>.</returns>
        /// <seealso cref="DrawHoveredInteractables"/>
        protected virtual bool ShouldDrawHoverMesh(MeshFilter meshFilter, Renderer meshRenderer, Camera mainCamera)
        {
            // TODO By only checking the main camera culling flags, but drawing the mesh in all cameras,
            // aren't we ignoring the culling mask of non-main cameras? Or does DrawMesh handle culling
            // automatically, making some of this evaluation unnecessary?
            var cullingMask = mainCamera.cullingMask;
            return meshFilter != null && (cullingMask & (1 << meshFilter.gameObject.layer)) != 0 && meshRenderer != null && meshRenderer.enabled;
        }

        /// <inheritdoc />
        protected internal override void OnRegistered(InteractorRegisteredEventArgs args)
        {
            base.OnRegistered(args);
            args.manager.interactableRegistered += OnInteractableRegistered;
            args.manager.interactableUnregistered += OnInteractableUnregistered;

            // Attempt to resolve any colliders that entered this trigger while this was not subscribed,
            // and filter out any targets that were unregistered while this was not subscribed.
            m_TriggerContactMonitor.interactionManager = args.manager;
            m_TriggerContactMonitor.ResolveUnassociatedColliders();
            XRInteractionManager.RemoveAllUnregistered(args.manager, m_ValidTargets);
        }

        /// <inheritdoc />
        protected internal override void OnUnregistered(InteractorUnregisteredEventArgs args)
        {
            base.OnUnregistered(args);
            args.manager.interactableRegistered -= OnInteractableRegistered;
            args.manager.interactableUnregistered -= OnInteractableUnregistered;
        }

        void OnInteractableRegistered(InteractableRegisteredEventArgs args)
        {
            m_TriggerContactMonitor.ResolveUnassociatedColliders(args.interactable);
        }

        void OnInteractableUnregistered(InteractableUnregisteredEventArgs args)
        {
            m_ValidTargets.Remove(args.interactable);
        }

        void OnContactAdded(XRBaseInteractable interactable)
        {
            if (!m_ValidTargets.Contains(interactable))
                m_ValidTargets.Add(interactable);
        }

        void OnContactRemoved(XRBaseInteractable interactable)
        {
            m_ValidTargets.Remove(interactable);
        }

        struct ShaderPropertyLookup
        {
            public static readonly int mode = Shader.PropertyToID("_Mode");
            public static readonly int srcBlend = Shader.PropertyToID("_SrcBlend");
            public static readonly int dstBlend = Shader.PropertyToID("_DstBlend");
            public static readonly int zWrite = Shader.PropertyToID("_ZWrite");
            public static readonly int baseColor = Shader.PropertyToID("_BaseColor");
            public static readonly int color = Shader.PropertyToID("_Color"); // Legacy
        }
    }
}
