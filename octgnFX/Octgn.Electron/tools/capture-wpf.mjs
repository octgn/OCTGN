/**
 * Capture a screenshot of the WPF OCTGN window using native Windows API via Node.
 * Usage: node tools/capture-wpf.mjs [output-filename]
 * Default: octgn-wpf-screenshot.png
 *
 * Requires: npm install screenshot-desktop (or uses fallback PowerShell)
 * For simplicity, this uses a child_process call to PowerShell as the actual
 * screenshot mechanism since Node doesn't have native Win32 window capture.
 */
import { execSync } from 'child_process';
import { existsSync } from 'fs';

const outputFile = process.argv[2] || 'octgn-wpf-screenshot.png';

// Use PowerShell inline — it's the simplest way to call Win32 APIs
const ps = `
Add-Type -AssemblyName System.Drawing.Common
Add-Type -AssemblyName System.Windows.Forms
$sig = '[DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);'
$sig += '[DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);'
$sig += '[DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out System.Drawing.Rectangle r);'
$type = Add-Type -MemberDefinition $sig -Name "W" -Namespace "T" -PassThru -ReferencedAssemblies System.Drawing.Primitives
$proc = Get-Process -Name "Octgn.JodsEngine" -EA SilentlyContinue | Select -First 1
if (!$proc) { $proc = Get-Process -Name "OCTGN" -EA SilentlyContinue | Where { $_.MainWindowTitle -ne '' } | Select -First 1 }
if (!$proc) { Write-Error "No OCTGN process"; exit 1 }
[T.W]::ShowWindow($proc.MainWindowHandle, 9) | Out-Null
[T.W]::SetForegroundWindow($proc.MainWindowHandle) | Out-Null
Start-Sleep -Milliseconds 800
$r = New-Object System.Drawing.Rectangle
[T.W]::GetWindowRect($proc.MainWindowHandle, [ref]$r) | Out-Null
$w = $r.Width - $r.X; $h = $r.Height - $r.Y
$bmp = New-Object System.Drawing.Bitmap($w, $h)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($r.X, $r.Y, 0, 0, (New-Object System.Drawing.Size($w, $h)))
$bmp.Save("${outputFile}"); $g.Dispose(); $bmp.Dispose()
Write-Host "ok"
`.replace(/\$/g, '$$$$').replace(/\n/g, ' ');

// Actually, simpler to just write to a temp .ps1 and run it
import { writeFileSync, unlinkSync } from 'fs';
import { tmpdir } from 'os';
import { join } from 'path';

const tmpPs1 = join(tmpdir(), `capture-wpf-${Date.now()}.ps1`);
const script = `
Add-Type -AssemblyName System.Drawing.Common
Add-Type -AssemblyName System.Windows.Forms
$sig = '[DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);'
$sig += '[DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);'
$sig += '[DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out System.Drawing.Rectangle r);'
$type = Add-Type -MemberDefinition $sig -Name "W" -Namespace "T" -PassThru -ReferencedAssemblies System.Drawing.Primitives
$proc = Get-Process -Name "Octgn.JodsEngine" -EA SilentlyContinue | Select-Object -First 1
if (-not $proc) { $proc = Get-Process -Name "OCTGN" -EA SilentlyContinue | Where-Object { $_.MainWindowTitle -ne '' } | Select-Object -First 1 }
if (-not $proc) { Write-Error "No OCTGN process"; exit 1 }
[T.W]::ShowWindow($proc.MainWindowHandle, 9) | Out-Null
[T.W]::SetForegroundWindow($proc.MainWindowHandle) | Out-Null
Start-Sleep -Milliseconds 800
$r = New-Object System.Drawing.Rectangle
[T.W]::GetWindowRect($proc.MainWindowHandle, [ref]$r) | Out-Null
$w = $r.Width - $r.X; $h = $r.Height - $r.Y
$bmp = New-Object System.Drawing.Bitmap($w, $h)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($r.X, $r.Y, 0, 0, (New-Object System.Drawing.Size($w, $h)))
$bmp.Save("${outputFile.replace(/\\/g, '/')}"); $g.Dispose(); $bmp.Dispose()
`;

writeFileSync(tmpPs1, script);

try {
  const result = execSync(`pwsh -File "${tmpPs1}"`, { encoding: 'utf-8', timeout: 10000 });
  if (existsSync(outputFile)) {
    console.log(`Screenshot saved: ${outputFile}`);
  } else {
    console.error('Screenshot file not created');
    console.error(result);
  }
} catch (e) {
  console.error('Failed:', e.stderr || e.message);
} finally {
  try { unlinkSync(tmpPs1); } catch {}
}
