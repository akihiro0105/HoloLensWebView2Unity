using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.WebView;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WebViewControl : MonoBehaviour
{
    [SerializeField]
    private WebView webView;
    [SerializeField]
    private string url;

    private float scrollSpeed = -10.0f;
    private float scrollLimit = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        IWebView web = null;
        webView.Load(url);
        webView.GetWebViewWhenReady(webview =>
        {
            web = webview;
            web.Resize(1280, 720);
        });

        var pointerHandler = new PointerHandler();
        pointerHandler.OnPointerClicked.RemoveAllListeners();
        pointerHandler.OnPointerDown.RemoveAllListeners();
        pointerHandler.OnPointerDragged.RemoveAllListeners();
        // Mouse Click
        var targetName = webView.gameObject.name;
        var isScroll = false;
        pointerHandler.OnPointerClicked.AddListener(async eventData =>
        {
            if (isScroll == true) return;
            if (eventData.Pointer.Result.CurrentPointerTarget == null || eventData.Pointer.Result.CurrentPointerTarget.name != targetName) return;
            var wmed = convertToWebViewSpace(web, eventData.Pointer.Result.Details.PointLocalSpace);
            wmed.Button = WebViewMouseEventData.MouseButton.ButtonLeft;
            wmed.Type = WebViewMouseEventData.EventType.MouseDown;
            (web as IWithMouseEvents)?.MouseEvent(wmed);
            await Task.Delay(10);
            wmed.Type = WebViewMouseEventData.EventType.MouseUp;
            (web as IWithMouseEvents)?.MouseEvent(wmed);
            Debug.Log("Click");
        });
        // Mouse Wheel
        var pointerDownPoint = new Vector3();
        pointerHandler.OnPointerDown.AddListener(eventData =>
        {
            if (eventData.Pointer.Result.CurrentPointerTarget == null || eventData.Pointer.Result.CurrentPointerTarget.name != targetName) return;
            pointerDownPoint = eventData.Pointer.Result.Details.PointLocalSpace;
            isScroll = false;
        });
        pointerHandler.OnPointerDragged.AddListener(eventData =>
        {
            if (eventData.Pointer.Result.CurrentPointerTarget == null || eventData.Pointer.Result.CurrentPointerTarget.name != targetName) return;
            var wmed = convertToWebViewSpace(web, eventData.Pointer.Result.Details.PointLocalSpace);
            var delta = eventData.Pointer.Result.Details.PointLocalSpace - pointerDownPoint;
            if (Mathf.Abs(delta.y) > scrollLimit) wmed.WheelY = delta.y * scrollSpeed;
            else if (Mathf.Abs(delta.x) > scrollLimit) wmed.WheelX = delta.x * scrollSpeed;
            else return;
            wmed.Type = WebViewMouseEventData.EventType.MouseWheel;
            (web as IWithMouseEvents)?.MouseEvent(wmed);
            isScroll = true;
            Debug.Log($"ScrollX : {wmed.WheelX} ScrollY : {wmed.WheelY}");
        });
        // set pointerhandler
        Microsoft.MixedReality.Toolkit.CoreServices.InputSystem.RegisterHandler<IMixedRealityPointerHandler>(pointerHandler);
    }

    private WebViewMouseEventData convertToWebViewSpace(IWebView web, Vector3 pos)
    {
        return new WebViewMouseEventData
        {
            X = (int)(web.Width * (pos.x + 0.5f)),
            Y = (int)(web.Height * (0.5f - pos.y)),
            TertiaryAxisDeviceType = WebViewMouseEventData.TertiaryAxisDevice.PointingDevice,
            Button = WebViewMouseEventData.MouseButton.ButtonNone,
            Device = WebViewMouseEventData.DeviceType.Mouse,
        };
    }
}

#if UNITY_EDITOR
public class EditorWebViewControl : MonoBehaviour
{
    [MenuItem("WebView/WindowTest")]
    static void Init()
    {
        var window = (EditorWebView)EditorWindow.GetWindow(typeof(EditorWebView));
        window.GetWebViewWhenReady(web => web.Load(new System.Uri("https://akihiro-document.azurewebsites.net/")));
        window.Show();
    }
}
#endif
