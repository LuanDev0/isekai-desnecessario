$root = Split-Path -Parent $MyInvocation.MyCommand.Path

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root\backend\IsekaiDesnecessario.API'; dotnet run"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root\frontend\isekai-desnecessario-app'; npm start"
