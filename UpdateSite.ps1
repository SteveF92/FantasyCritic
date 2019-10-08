dotnet publish -c Release -o C:\FantasyCritic\BuildArea\
Write-Output "Stopping site"
Stop-Website "FantasyCritic"
Write-Output "Deleting files"
while((Get-ChildItem "C:\FantasyCritic\Binary\" | Measure-Object).Count -gt 0)
{
    Get-ChildItem C:\FantasyCritic\Binary\ -Recurse | Remove-Item -Recurse -Force
}
$numFiles = (Get-ChildItem "C:\FantasyCritic\Binary\" | Measure-Object).Count
Write-Output "Delete done, $numFiles in folder."
Write-Output "Copying files"
Copy-Item -Path "C:\FantasyCritic\BuildArea\*" -Destination "C:\FantasyCritic\Binary\" -Force -Recurse
Write-Output "Copying Items"
Copy-Item -Path "C:\Users\Administrator\Desktop\CopyFiles\appsettings.json" -Destination "C:\FantasyCritic\Binary\" -Force
Copy-Item -Path "C:\Users\Administrator\Desktop\CopyFiles\EmailTemplates" -Destination "C:\FantasyCritic\Binary\" -Force -Recurse
Write-Output "Starting site"
Start-Website "FantasyCritic"
Write-Output "Update finished"