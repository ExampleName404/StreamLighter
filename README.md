# StreamLighter — Virtual Ring Light Overlay (WPF)

StreamLighter uses your monitor as a soft, edge-lit ring light to improve webcam lighting during calls and recordings. It draws a soft glowing frame around the chosen monitor and can punch a transparent hole around the cursor so you can inspect the area under the pointer.

**Creator:** by NotUnrealEngineer

Features
- Toggle overlay on/off
- Adjust brightness (opacity)
- Select which monitor to use
- Choose which edges to light (Top / Bottom / Left / Right) and set thickness
- Transparent circular hole around the cursor appears only when the cursor is over the lit frame
- Click-through overlay (doesn't block apps)
- System tray icon with Open / Exit

Quick start (run from source)

Requirements: .NET SDK (9.0 or later recommended) on Windows.

```powershell
# build
dotnet build

# run
dotnet run --project .
```

Run the published single-file executable

I publish a self-contained single-file executable for Windows x64 into the `publish\win-x64` folder. After publishing you can run the exe directly without installing .NET:

```powershell
# run the produced executable
.
\publish\win-x64\StreamLighter.exe
```

Usage notes
- The overlay is click-through by design. To change settings use the small control window.
- Closing the control window hides the app to the system tray. Choose Exit from the tray menu to quit.
- Use the monitor selector to move the overlay to a different display.

Troubleshooting
- If the overlay appears on the wrong monitor after toggling, open the control window, choose the desired monitor from the list, then toggle the overlay off and on — the app will place the overlay on the selected display. Recent fixes make this behavior more reliable.
- If the executable is blocked or flagged by antivirus, mark it as safe or code-sign the binary for distribution.

Development notes
- The project targets modern .NET and uses WPF. The publish step used in development was:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=false -o publish\win-x64
```

Files of interest
- Main UI: [MainWindow.xaml](MainWindow.xaml#L1)
- Overlay: [OverlayWindow.xaml](OverlayWindow.xaml#L1)
- Project file: [StreamLighter.csproj](StreamLighter.csproj)
- Published exe: [publish\win-x64](publish/win-x64)

License & credits
- Created by NotUnrealEngineer. Use and modify freely for personal projects. If you redistribute, please keep the credit.

Want more?
- I can add presets, hotkeys, color temperature controls, or an installer. Tell me which feature to add next.

