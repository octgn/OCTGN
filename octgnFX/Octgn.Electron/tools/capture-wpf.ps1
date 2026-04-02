# Capture a screenshot of the WPF OCTGN game window.
# Uses Windows.Graphics.Capture via .NET to take a window-specific screenshot.
# Fallback: CopyFromScreen after bringing window to front.
# Usage: pwsh -File tools/capture-wpf.ps1 [output-filename]
param(
    [string]$OutputFile = "octgn-wpf-screenshot.png"
)

Add-Type -AssemblyName System.Drawing.Common
Add-Type -AssemblyName System.Windows.Forms

# Find OCTGN game window
$proc = Get-Process -Name "Octgn.JodsEngine" -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $proc) {
    $proc = Get-Process -Name "OCTGN" -ErrorAction SilentlyContinue |
            Where-Object { $_.MainWindowTitle -ne '' } |
            Select-Object -First 1
}
if (-not $proc) {
    Write-Error "No OCTGN process found"
    exit 1
}

Write-Host "Capturing: $($proc.MainWindowTitle) (PID $($proc.Id))"

# Bring window to front
$sig = '[DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);'
$sig += '[DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);'
$sig += '[DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out System.Drawing.Rectangle r);'
$type = Add-Type -MemberDefinition $sig -Name "Win32Capture" -Namespace "Temp" -PassThru -ReferencedAssemblies System.Drawing.Primitives

[Temp.Win32Capture]::ShowWindow($proc.MainWindowHandle, 9) | Out-Null
[Temp.Win32Capture]::SetForegroundWindow($proc.MainWindowHandle) | Out-Null
Start-Sleep -Milliseconds 800

$rect = New-Object System.Drawing.Rectangle
[Temp.Win32Capture]::GetWindowRect($proc.MainWindowHandle, [ref]$rect) | Out-Null

# Rectangle from GetWindowRect: X=Left, Y=Top, Width=Right, Height=Bottom (it's a RECT, not a Rectangle)
$left = $rect.X
$top = $rect.Y
$width = $rect.Width - $rect.X
$height = $rect.Height - $rect.Y

Write-Host "Window: ${width}x${height} at ($left,$top)"

$bmp = New-Object System.Drawing.Bitmap($width, $height)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($left, $top, 0, 0, (New-Object System.Drawing.Size($width, $height)))
$bmp.Save($OutputFile)
$g.Dispose()
$bmp.Dispose()
Write-Host "Saved: $OutputFile"
