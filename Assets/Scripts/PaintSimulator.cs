using UnityEngine;
using System.IO;
using System;

public class PaintSimulator : MonoBehaviour
{
    [Header("Configuración del Canvas")]
    public int canvasWidth = 800;
    public int canvasHeight = 600;
    public Color backgroundColor = Color.white;
    
    [Header("Herramientas")]
    public int brushSize = 5;
    public Color currentColor = Color.black;
    
    private Texture2D canvas;
    private Texture2D backupCanvas; // Para deshacer
    private bool isDrawing = false;
    private Vector2 lastDrawPos;
    
    private enum Tool { Pencil, Brush, Eraser, Fill, Line, Rectangle, Circle, Eyedropper }
    private Tool currentTool = Tool.Pencil;
    
    private Vector2 lineStartPos;
    private bool isDrawingShape = false;
    
    // Paleta de colores predefinida (como Paint clásico)
    private Color[] colorPalette = new Color[]
    {
        // Fila 1
        Color.black, new Color(0.5f, 0.5f, 0.5f), new Color(0.5f, 0, 0), new Color(0.5f, 0.25f, 0),
        new Color(0.5f, 0.5f, 0), new Color(0, 0.5f, 0), new Color(0, 0.5f, 0.5f), new Color(0, 0, 0.5f),
        new Color(0.25f, 0, 0.5f), new Color(0.5f, 0, 0.5f),
        // Fila 2
        Color.white, new Color(0.75f, 0.75f, 0.75f), Color.red, new Color(1, 0.5f, 0),
        Color.yellow, Color.green, Color.cyan, Color.blue,
        new Color(0.5f, 0, 1), Color.magenta,
        // Colores adicionales
        new Color(1, 0.75f, 0.8f), new Color(0.6f, 0.4f, 0.2f), new Color(1, 0.9f, 0.6f), new Color(0.8f, 1, 0.8f),
        new Color(0.6f, 1, 1), new Color(0.8f, 0.8f, 1), new Color(1, 0.8f, 1), new Color(1, 0.6f, 0.6f)
    };
    
    void Start()
    {
        InitializeCanvas();
    }
    
    void InitializeCanvas()
    {
        canvas = new Texture2D(canvasWidth, canvasHeight);
        backupCanvas = new Texture2D(canvasWidth, canvasHeight);
        
        // Llenar con color de fondo
        Color[] pixels = new Color[canvasWidth * canvasHeight];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = backgroundColor;
        }
        canvas.SetPixels(pixels);
        canvas.Apply();
        
        // Crear sprite y asignarlo
        GetComponent<SpriteRenderer>().sprite = Sprite.Create(
            canvas,
            new Rect(0, 0, canvasWidth, canvasHeight),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        // Obtener posición del mouse en el canvas
        Vector2 mousePos = GetCanvasMousePosition();
        
        if (mousePos.x < 0 || mousePos.x >= canvasWidth || mousePos.y < 0 || mousePos.y >= canvasHeight)
            return;
        
        // Dibujar con clic izquierdo
        if (Input.GetMouseButtonDown(0))
        {
            BackupCanvas();
            
            if (currentTool == Tool.Line || currentTool == Tool.Rectangle || currentTool == Tool.Circle)
            {
                lineStartPos = mousePos;
                isDrawingShape = true;
            }
            else if (currentTool == Tool.Fill)
            {
                FloodFill((int)mousePos.x, (int)mousePos.y, currentColor);
            }
            else if (currentTool == Tool.Eyedropper)
            {
                currentColor = canvas.GetPixel((int)mousePos.x, (int)mousePos.y);
            }
            else
            {
                isDrawing = true;
                Draw(mousePos);
            }
            
            lastDrawPos = mousePos;
        }
        
        if (Input.GetMouseButton(0) && isDrawing)
        {
            DrawLine(lastDrawPos, mousePos);
            lastDrawPos = mousePos;
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            if (isDrawingShape)
            {
                if (currentTool == Tool.Line)
                    DrawLine(lineStartPos, mousePos);
                else if (currentTool == Tool.Rectangle)
                    DrawRectangle(lineStartPos, mousePos);
                else if (currentTool == Tool.Circle)
                    DrawCircle(lineStartPos, mousePos);
                
                isDrawingShape = false;
            }
            
            isDrawing = false;
            canvas.Apply();
        }
        
        // Teclas de atajo
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentTool = Tool.Pencil;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentTool = Tool.Brush;
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentTool = Tool.Eraser;
        if (Input.GetKeyDown(KeyCode.Alpha4)) currentTool = Tool.Fill;
        if (Input.GetKeyDown(KeyCode.Alpha5)) currentTool = Tool.Line;
        if (Input.GetKeyDown(KeyCode.Alpha6)) currentTool = Tool.Rectangle;
        if (Input.GetKeyDown(KeyCode.Alpha7)) currentTool = Tool.Circle;
        if (Input.GetKeyDown(KeyCode.Alpha8)) currentTool = Tool.Eyedropper;
        
        // Cambiar tamaño de pincel
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
            brushSize = Mathf.Min(brushSize + 2, 50);
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            brushSize = Mathf.Max(brushSize - 2, 1);
        
        // Deshacer
        if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl))
            RestoreCanvas();
        
        // Limpiar canvas
        if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftControl))
            ClearCanvas();
        
        // Guardar
        if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftControl))
            SaveImage();
    }
    
    Vector2 GetCanvasMousePosition()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 spritePos = transform.InverseTransformPoint(worldPos);
        
        float x = (spritePos.x + canvasWidth / 200f) * 100f;
        float y = (spritePos.y + canvasHeight / 200f) * 100f;
        
        return new Vector2(x, y);
    }
    
    void Draw(Vector2 pos)
    {
        int size = currentTool == Tool.Eraser ? brushSize * 2 : brushSize;
        Color color = currentTool == Tool.Eraser ? backgroundColor : currentColor;
        
        for (int x = -size; x <= size; x++)
        {
            for (int y = -size; y <= size; y++)
            {
                if (currentTool == Tool.Brush || currentTool == Tool.Eraser)
                {
                    // Pincel circular suave
                    if (x * x + y * y <= size * size)
                    {
                        int px = (int)pos.x + x;
                        int py = (int)pos.y + y;
                        if (px >= 0 && px < canvasWidth && py >= 0 && py < canvasHeight)
                            canvas.SetPixel(px, py, color);
                    }
                }
                else if (currentTool == Tool.Pencil)
                {
                    // Lápiz pequeño y preciso
                    if (x * x + y * y <= 4)
                    {
                        int px = (int)pos.x + x;
                        int py = (int)pos.y + y;
                        if (px >= 0 && px < canvasWidth && py >= 0 && py < canvasHeight)
                            canvas.SetPixel(px, py, color);
                    }
                }
            }
        }
        canvas.Apply();
    }
    
    void DrawLine(Vector2 start, Vector2 end)
    {
        int x0 = (int)start.x;
        int y0 = (int)start.y;
        int x1 = (int)end.x;
        int y1 = (int)end.y;
        
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        
        while (true)
        {
            Draw(new Vector2(x0, y0));
            
            if (x0 == x1 && y0 == y1) break;
            
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
    
    void DrawRectangle(Vector2 start, Vector2 end)
    {
        RestoreCanvas();
        
        int x1 = (int)Mathf.Min(start.x, end.x);
        int x2 = (int)Mathf.Max(start.x, end.x);
        int y1 = (int)Mathf.Min(start.y, end.y);
        int y2 = (int)Mathf.Max(start.y, end.y);
        
        for (int x = x1; x <= x2; x++)
        {
            Draw(new Vector2(x, y1));
            Draw(new Vector2(x, y2));
        }
        
        for (int y = y1; y <= y2; y++)
        {
            Draw(new Vector2(x1, y));
            Draw(new Vector2(x2, y));
        }
    }
    
    void DrawCircle(Vector2 center, Vector2 edge)
    {
        RestoreCanvas();
        
        int radius = (int)Vector2.Distance(center, edge);
        int cx = (int)center.x;
        int cy = (int)center.y;
        
        for (int angle = 0; angle < 360; angle += 1)
        {
            float rad = angle * Mathf.Deg2Rad;
            int x = cx + (int)(radius * Mathf.Cos(rad));
            int y = cy + (int)(radius * Mathf.Sin(rad));
            Draw(new Vector2(x, y));
        }
    }
    
    void FloodFill(int x, int y, Color fillColor)
    {
        Color targetColor = canvas.GetPixel(x, y);
        
        if (targetColor == fillColor) return;
        
        System.Collections.Generic.Queue<Vector2Int> queue = new System.Collections.Generic.Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(x, y));
        
        int fillCount = 0;
        int maxFill = 100000; // Límite de seguridad
        
        while (queue.Count > 0 && fillCount < maxFill)
        {
            Vector2Int pos = queue.Dequeue();
            
            if (pos.x < 0 || pos.x >= canvasWidth || pos.y < 0 || pos.y >= canvasHeight)
                continue;
            
            Color pixelColor = canvas.GetPixel(pos.x, pos.y);
            
            if (pixelColor == targetColor)
            {
                canvas.SetPixel(pos.x, pos.y, fillColor);
                fillCount++;
                
                queue.Enqueue(new Vector2Int(pos.x + 1, pos.y));
                queue.Enqueue(new Vector2Int(pos.x - 1, pos.y));
                queue.Enqueue(new Vector2Int(pos.x, pos.y + 1));
                queue.Enqueue(new Vector2Int(pos.x, pos.y - 1));
            }
        }
        
        canvas.Apply();
    }
    
    void BackupCanvas()
    {
        backupCanvas.SetPixels(canvas.GetPixels());
        backupCanvas.Apply();
    }
    
    void RestoreCanvas()
    {
        canvas.SetPixels(backupCanvas.GetPixels());
        canvas.Apply();
    }
    
    void ClearCanvas()
    {
        BackupCanvas();
        Color[] pixels = new Color[canvasWidth * canvasHeight];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = backgroundColor;
        }
        canvas.SetPixels(pixels);
        canvas.Apply();
    }
    
    public void SaveImage()
    {
        byte[] bytes = canvas.EncodeToPNG();
        string filename = "PaintDrawing_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        string path = Application.persistentDataPath + "/" + filename;
        File.WriteAllBytes(path, bytes);
        Debug.Log("Imagen guardada en: " + path);
    }
    
    public void SetColor(int index)
    {
        if (index >= 0 && index < colorPalette.Length)
            currentColor = colorPalette[index];
    }
    
    public void SetTool(int toolIndex)
    {
        currentTool = (Tool)toolIndex;
    }
    
    public void SetBrushSize(int size)
    {
        brushSize = Mathf.Clamp(size, 1, 50);
    }
    
    // Getters para UI
    public string GetCurrentToolName() => currentTool.ToString();
    public int GetBrushSize() => brushSize;
    public Color GetCurrentColor() => currentColor;
    public Color[] GetColorPalette() => colorPalette;
}