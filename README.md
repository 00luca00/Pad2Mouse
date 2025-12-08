# MouseController 🎮🖱️

MouseController is a Windows tool that lets you use a **game controller as a mouse**, allowing analog stick cursor movement and button-based mouse clicks.  
It is designed mainly for Xbox controllers, and PlayStation controller support is planned but not yet functional.

---

## Features

- Move the mouse cursor using the controller's analog stick  
- Perform left/right mouse clicks using controller buttons  
- Smooth movement and responsiveness  
- Works on **Windows** with **Xbox controllers**  
- Built using **C#** and **WPF (.NET Framework)**  

---

## Current Status

| Feature                                  | Status             |
|------------------------------------------|--------------------|
| Cursor movement with analog stick        | ✅ Working          |
| Left / right mouse click                 | ✅ Working          |
| Xbox controller support                  | ✅ Working          |
| PlayStation controller support           | ⚠️ Not implemented |
| Scrolling support                        | ⚠️ Not implemented |
| On-screen configuration UI               | ⚠️ Not implemented |

---

## Technologies Used

- **C# / .NET Framework**
- **WPF** for the graphical interface  
- **Win32 API** for mouse control  
- **XInput** for Xbox controller input (PlayStation support will require a different API)

---

## Installation & Usage

### 1. Clone the repository

```bash
git clone https://github.com/00luca00/MouseController.git
cd MouseController
