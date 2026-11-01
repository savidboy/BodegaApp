param(
    [string]$Url = "http://localhost:5098"
)

if (-not (Get-Command cloudflared -ErrorAction SilentlyContinue)) {
    Write-Host "cloudflared no esta instalado." -ForegroundColor Yellow
    Write-Host "Instalalo con: winget install --id Cloudflare.cloudflared"
    exit 1
}

Write-Host "Abriendo el tunel hacia $Url ..." -ForegroundColor Cyan
Write-Host "Copia la URL https://....trycloudflare.com que aparece abajo."
cloudflared tunnel --url $Url
