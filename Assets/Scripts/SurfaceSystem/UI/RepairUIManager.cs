using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using SurfaceSystem.Data;
using SurfaceSystem.Events;
using SurfaceSystem.Components;
using SurfaceSystem.UI.MaterialButton;
using SurfaceSystem.UI.SurfaceButton;
using System.Linq;

namespace SurfaceSystem.UI
{
    public class RepairUIManager : MonoBehaviour
    {
        private static RepairUIManager _instance;
        public static RepairUIManager Instance => _instance;

        //[SerializeField] private List<BaseSurfaceController> _targetSurfaces = new ();

        [Header("Контейнер с поверхностями (дочерние объекты - BaseSurfaceController)")]
        [SerializeField] private Transform _surfacesContainer;

        [Header("Surface Panels")]
        [SerializeField] private SurfacePanelData _ceilingSurfacePanel;
        [SerializeField] private SurfacePanelData _wallSurfacePanel;
        [SerializeField] private SurfacePanelData _floorSurfacePanel;        

        [Header("Material Panel - 1 панель")]
        [SerializeField] private SurfacePanelData _materialPanel;
        
        [Header("Button Prefabs")]
        [SerializeField] private GameObject _surfaceButtonPrefab;
        [SerializeField] private GameObject _materialButtonPrefab;        

        private BaseSurfaceController _selectedSurface;
        private List<MaterialDefinition> _allMaterials;

        private List<BaseSurfaceController> _targetSurfaces = new();

        private bool _isShowAllMaterials = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            RefreshSurfaces();

            /*if (_targetSurfaces.Count == 0)
            {
                _targetSurfaces.AddRange(FindObjectsOfType<BaseSurfaceController>());
            }     */       
            
            LoadAllMaterials();
            InitializePanels();
            ShowSurfacePanel(SurfaceType.Wall);

            SurfaceEvents.OnMaterialFilterChanged += OnMaterialFilterChanged;            
            
            if (_materialPanel.panel != null)
                _materialPanel.panel.SetActive(false);
        }

        public void RefreshSurfaces()
        {
            _targetSurfaces.Clear();
            
            if (_surfacesContainer != null)
            {
                // Рекурсивно собираем все BaseSurfaceController во всех дочерних объектах
                GetAllSurfacesRecursive(_surfacesContainer, _targetSurfaces);
            }
            else
            {
                // fallback – все поверхности в сцене
                _targetSurfaces.AddRange(FindObjectsOfType<BaseSurfaceController>());
            }

            // Сортировка для стабильной нумерации
            _targetSurfaces = _targetSurfaces
                .OrderBy(s => s.SurfaceType)
                .ThenBy(s => s.name)
                .ToList();

            // Перезагружаем материалы
            LoadAllMaterials();
            InitializePanels();

            if (_materialPanel.panel != null && _materialPanel.panel.activeSelf)
                CreateMaterialButtons();
        }

        private void GetAllSurfacesRecursive(Transform parent, List<BaseSurfaceController> list)
        {
            // Проверяем текущий объект
            var surface = parent.GetComponent<BaseSurfaceController>();
            if (surface != null)
                list.Add(surface);

            // Рекурсивно проверяем всех детей
            foreach (Transform child in parent)
            {
                GetAllSurfacesRecursive(child, list);
            }
        }
        
        private void LoadAllMaterials()
        {
            _allMaterials = new List<MaterialDefinition>();
            
            foreach (var surface in _targetSurfaces)
            {
                var surfaceMaterials = surface.GetAllMaterialsFromLayers();
                foreach (var material in surfaceMaterials)
                {
                    if (material != null && material.MaterialType != MaterialType.Base)
                    {
                        if (!_allMaterials.Contains(material))
                            _allMaterials.Add(material);
                    }
                }
            }
            
            Debug.Log($"Загружено материалов: {_allMaterials.Count}");            
           
            for (int i = _allMaterials.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (_allMaterials[i], _allMaterials[j]) = (_allMaterials[j], _allMaterials[i]);
            }
        }
        
        private void InitializePanels()
        {
            CreateSurfaceButtons(SurfaceType.Wall, _wallSurfacePanel.buttonsContainer);
            CreateSurfaceButtons(SurfaceType.Floor, _floorSurfacePanel.buttonsContainer);
            CreateSurfaceButtons(SurfaceType.Ceiling, _ceilingSurfacePanel.buttonsContainer);
        }
        
        private void CreateSurfaceButtons(SurfaceType surfaceType, Transform container)
        {
            if (container == null) return;
            
            ClearContainer(container);
            
            int number = 1;
            foreach (var surface in _targetSurfaces)
            {
                if (surface.SurfaceType == surfaceType)
                {
                    GameObject button = Instantiate(_surfaceButtonPrefab, container);
                    SurfaceButtonController controller = button.GetComponent<SurfaceButtonController>();
                    if (controller != null)
                    {
                        controller.Initialize(surface, number);
                        number++;
                    }
                }
            }
        }
        
        private void CreateMaterialButtons()
        {
            if (_materialPanel.buttonsContainer == null) return;
    
            ClearContainer(_materialPanel.buttonsContainer);
            
            IEnumerable<MaterialDefinition> materialsToShow = _allMaterials;

            if(!_isShowAllMaterials && _selectedSurface != null)
            {
                var surfaceTypeDef = _selectedSurface.SurfaceTypeDefinition;
                materialsToShow = _allMaterials.Where(m => m.AllowedSurfaceType == null || m.AllowedSurfaceType == surfaceTypeDef);
            }

            materialsToShow = materialsToShow.Where(m => m.MaterialType != MaterialType.Base);
            
            foreach (var material in materialsToShow)
            {
                Debug.Log($"Создаю кнопку для: {material.DisplayName}");
                
                GameObject button = Instantiate(_materialButtonPrefab, _materialPanel.buttonsContainer);
                MaterialButtonController controller = button.GetComponent<MaterialButtonController>();
                if (controller != null)
                {
                    controller.Initialize(material);
                }
            }
        }
        
        private void ClearContainer(Transform container)
        {
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }
        
        public void ShowSurfacePanel(SurfaceType type)
        {
            if (_wallSurfacePanel.panel != null) _wallSurfacePanel.panel.SetActive(false);
            if (_floorSurfacePanel.panel != null) _floorSurfacePanel.panel.SetActive(false);
            if (_ceilingSurfacePanel.panel != null) _ceilingSurfacePanel.panel.SetActive(false);
            if (_materialPanel.panel != null) _materialPanel.panel.SetActive(false);
            
            switch (type)
            {
                case SurfaceType.Wall: 
                    if (_wallSurfacePanel.panel != null) _wallSurfacePanel.panel.SetActive(true); 
                    break;
                case SurfaceType.Floor: 
                    if (_floorSurfacePanel.panel != null) _floorSurfacePanel.panel.SetActive(true); 
                    break;
                case SurfaceType.Ceiling: 
                    if (_ceilingSurfacePanel.panel != null) _ceilingSurfacePanel.panel.SetActive(true); 
                    break;
            }
        }
        
        public void ShowMaterialPanel(SurfaceType type)
        {
            if (_selectedSurface == null)
            {
                Debug.LogWarning("Сначала выберите поверхность");
                return;
            }
            
            if (_wallSurfacePanel.panel != null) _wallSurfacePanel.panel.SetActive(false);
            if (_floorSurfacePanel.panel != null) _floorSurfacePanel.panel.SetActive(false);
            if (_ceilingSurfacePanel.panel != null) _ceilingSurfacePanel.panel.SetActive(false);
            
            UpdateMaterialPanelTitle();
            
            CreateMaterialButtons();
            
            if (_materialPanel.panel != null)
                _materialPanel.panel.SetActive(true);
        }
        
        public void CloseMaterialPanel()
        {
            if (_materialPanel.panel != null)
                _materialPanel.panel.SetActive(false);
            
            if (_selectedSurface != null)
            {
                ShowSurfacePanel(_selectedSurface.SurfaceType);
            }
        }

        private void UpdateMaterialPanelTitle()
        {
            if (_materialPanel.title == null || _selectedSurface == null) return;

            string typeName = GetSurfaceTypeName(_selectedSurface);
            int surfaceNumber = GetSurfaceNumber(_selectedSurface);

            _materialPanel.title.text = $"{typeName} {surfaceNumber}";
        }

        private string GetSurfaceTypeName(BaseSurfaceController surface)
        {
            switch (surface.SurfaceType)
            {
                case SurfaceType.Wall: return "Стена";
                case SurfaceType.Floor: return "Пол";
                case SurfaceType.Ceiling: return "Потолок";
                default: return "Поверхность";
            }
        }

        private int GetSurfaceNumber(BaseSurfaceController surface)
        {
            int number = 1;
            foreach (var s in _targetSurfaces)
            {
                if (s.SurfaceType == surface.SurfaceType)
                {
                    if (s == surface) return number;
                    number++;
                }
            }
            return number;
        }

        public void SelectWall(BaseSurfaceController surface)
        {           
            
            if (_selectedSurface != null)
                _selectedSurface.SetHighlight(false);
            
            _selectedSurface = surface;
            
            if (_selectedSurface != null)
                _selectedSurface.SetHighlight(true);
            
            SurfaceEvents.RaiseWallSelected(surface);
        }

        public void SetSelectedMaterial(MaterialDefinition material)
        {
            if (material == null || _selectedSurface == null)
            {
                Debug.LogWarning("Сначала выберите поверхность");
                SurfaceEvents.RaiseLayerError(_selectedSurface, "Сначала выберите поверхность!");
                return;
            }
            _selectedSurface.TryApplyMaterial(material);
        }

        public void ResetSelectedSurface()
        {
            if (_selectedSurface != null) _selectedSurface.ResetSurface();
        }

        private void OnMaterialFilterChanged(bool showAllMaterials)
        {
            _isShowAllMaterials = showAllMaterials;

            string message = showAllMaterials ? "Режим: Показать все материалы" : "Режим: Оставить только подходящие материалы";
            SurfaceEvents.RaiseNotification(message);
            
            // обновлние панели материалов, если она открыта
            if (_materialPanel.panel != null && _materialPanel.panel.activeSelf && _selectedSurface != null)
            {
                CreateMaterialButtons();
            }
        }

        private void OnDestroy()
        {
            SurfaceEvents.OnMaterialFilterChanged -= OnMaterialFilterChanged;
        }
        
        public void ShowWallSurfacePanel() => ShowSurfacePanel(SurfaceType.Wall);
        public void ShowFloorSurfacePanel() => ShowSurfacePanel(SurfaceType.Floor);
        public void ShowCeilingSurfacePanel() => ShowSurfacePanel(SurfaceType.Ceiling);
        
        public void OnCloseMaterialPanel() => CloseMaterialPanel();

        public List<BaseSurfaceController> GetTargetSurfaces() => _targetSurfaces;
    }
}