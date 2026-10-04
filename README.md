# MyAutoSuperFastClicker ⚡

A high-performance, low-latency, ultra-fast Windows auto-clicker built with **C# .NET 8** and **WPF**. Engineered specifically for clicker games, idle games, and rapid repetitive tasks where speed and responsiveness are critical.

---

## ✨ Features

- 🚀 **Extreme Turbo Mode**: Fires clicks as fast as Windows and hardware allow using direct OS-level Win32 `SendInput`, reaching 1,000+ Clicks Per Second (CPS).
- 🎯 **Configurable Target CPS**: Choose your exact desired click rate (e.g. 20, 50, 100, 250, 500, 1000 CPS) with sub-millisecond precision.
- ⏱ **Millisecond Interval**: Set exact delay in milliseconds between clicks.
- ⌨️ **Global Hotkeys**:
  - **`F6`**: Start / Stop clicking toggle anywhere on Windows (even during full-screen games).
  - **`F7`**: Instantly capture the current mouse cursor location for locked coordinate clicking.
- 📍 **Click Position Modes**:
  - **Current Cursor**: Clicks wherever you move the mouse in real time.
  - **Fixed Coordinates**: Locks clicking to specific screen coordinates `(X, Y)`.
- 🖱 **Mouse Buttons & Actions**:
  - Left Click, Right Click, Middle Click.
  - Single Click or Double Click.
- 🔁 **Repeat Controls**:
  - Infinite (until stopped with `F6`).
  - Exact count limit (e.g., stop after 500 clicks).
- 📌 **Always On Top**: Pin the window above your game window.
- 📊 **Real-Time Live Dashboard**: Shows active status, live measured CPS, and total click counter.

---

## 🚀 How to Run

### Prerequisites
- Windows 10 or 11
- [.NET 8 SDK or Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

### Quick Start
1. Open PowerShell or Terminal in the project root:
   ```powershell
   dotnet run
   ```
2. Or build the Release executable:
   ```powershell
   dotnet build -c Release
   ```
   The executable will be located at:
   `bin\Release\net8.0-windows\MyAutoSuperFastClicker.exe`

---

## 🎮 How to Cheat Clicker Games

1. Launch `MyAutoSuperFastClicker`.
2. Open your favorite clicker game (e.g., Cookie Clicker, Clicker Heroes, etc.).
3. Choose **🚀 UNCHOPPED TURBO (Max Speed)** or set **Target CPS** to `500` or `1000`.
4. Hover your mouse over the big cookie / target monster and press **`F6`**.
5. Watch your score and upgrades skyrocket! Press **`F6`** again to stop.
6. Alternatively, move the mouse over an upgrade button, press **`F7`** to lock the coordinates, and toggle **`F6`** to automate buying upgrades without moving your cursor.

---

## 🧪 Testing

Run automated unit tests:
```powershell
dotnet test
```
All tests verify delay calculations, timing logic, engine lifecycles, and repeat count boundaries.
