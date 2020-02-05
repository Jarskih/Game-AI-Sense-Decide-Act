using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FlatEarth
{
    public class CreateUIElement : MonoBehaviour
    {
        private Camera _camera;
        private GameObject _uiElement;
        private Sprite _grass;
        private Sprite _wolf;
        private Sprite _sheep;
    void Start()
    {
        _uiElement = Resources.Load<GameObject>("Prefabs/UIElement");
        _grass = Resources.Load<Sprite>("Sprites/Grass");
        _wolf = Resources.Load<Sprite>("Sprites/Wolf");
        _sheep = Resources.Load<Sprite>("Sprites/Sheep");
        _camera = (Camera) FindObjectOfType(typeof(Camera));
    }
    
    public void StartListeningForEvents()
    {
        EventManager.StartListening("SheepUI", CreateSheepIcon);
        EventManager.StartListening("WolfUI", CreateWolfIcon);
        EventManager.StartListening("GrassUI", CreateGrassIcon);
    }
    
    void OnDisable()
    {
        EventManager.StopListening("SheepUI", CreateSheepIcon);
        EventManager.StopListening("WolfUI", CreateWolfIcon);
        EventManager.StopListening("GrassUI", CreateGrassIcon);
    }

    private void CreateSheepIcon(EventManager.EventMessage args)
    {
        var element = Instantiate(_uiElement, this.transform);
        element.GetComponent<Image>().sprite = _sheep;
    }
    
    private void CreateWolfIcon(EventManager.EventMessage args)
    {
        var element = Instantiate(_uiElement, this.transform);
        element.GetComponent<Image>().sprite = _wolf;
        element.transform.position = _camera.WorldToScreenPoint(args.node.GetNodeWorldPos());
    }
    
    private void CreateGrassIcon(EventManager.EventMessage args)
    {
        var element = Instantiate(_uiElement, this.transform);
        element.GetComponent<Image>().sprite = _grass;
        element.transform.position = _camera.WorldToScreenPoint(args.node.GetNodeWorldPos());
    }
    
}
}