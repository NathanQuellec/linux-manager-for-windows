# Linux Manager

A modern, open-source desktop application for managing Windows Subsystem for Linux (WSL) distributions on Windows. Built with .NET 8 and WinUI 3.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Release](https://img.shields.io/github/v/release/NathanQuellec/linux-manager-for-windows)](https://github.com/NathanQuellec/linux-manager-for-windows/releases)
[![Build Status](https://github.com/NathanQuellec/linux-manager-for-windows/actions/workflows/ci.yml/badge.svg)](https://github.com/NathanQuellec/linux-manager-for-windows/actions)

## Overview

Linux Manager provides an intuitive graphical interface for common WSL operations — view, create, snapshot, and manage your distributions without touching the command line.

## ✨ Features

### Manage Distributions

Easily access detailed information about existing WSL distributions, including their names, versions, and operating systems.

<img width="3008" height="1680" alt="homepage-linuxmanager" src="https://github.com/user-attachments/assets/7479080a-c6d8-40a9-bb0d-6fddb2f55505" />

<br><br>

### Create Distributions from Docker Images

Create new WSL distributions by specifying a Docker Hub repository, even **<ins>without Docker installed on your host system</ins>**. You can also create distributions from a Dockerfile (requires Docker Desktop).

<img width="3009" height="1679" alt="create-wsl-distribution-from-docker-image" src="https://github.com/user-attachments/assets/e7a4e861-00ba-480e-a029-21de859989ed" />

<br><br>

### Create Snapshots from Existing Distributions

Save your work before updating your favorite distribution. Linux Manager builds snapshots for you with ease.

<img width="3014" height="1678" alt="create-wsl-distribution-snapshot" src="https://github.com/user-attachments/assets/6306d8ea-a0ca-4e63-902d-674c2fd3e590" />

## 🔧 Requirements

- **Windows 10** (Build 19041 / May 2020 Update) or **Windows 11**
- **.NET 8 Runtime**
- **WSL** enabled on your system ([Installation Guide](https://docs.microsoft.com/en-us/windows/wsl/install))

## 📥 Installation

### Option 1: Microsoft Store (Recommended)

[Download from Microsoft Store](https://apps.microsoft.com/detail/9plsjr4tg2gq?hl=en-us&gl=EN)

### Option 2: GitHub Release

1. Download the latest release from [GitHub Releases](https://github.com/NathanQuellec/linux-manager-for-windows/releases)
2. Extract the archive to your desired location
3. Run `Linux Manager.exe`

### Option 3: Build from Source

See [Building from Source](#building-from-source) below.

## 🏗️ Building from Source

### Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/) with:
  - .NET 8 SDK
  - Windows App SDK workload
- WSL enabled

### Build Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/NathanQuellec/linux-manager-for-windows.git
   cd linux-manager-for-windows
   ```

2. **Open the solution** in Visual Studio 2022

3. **Build and run** with `F5` or via `Debug > Start Debugging`

## 🧪 Testing

```bash
dotnet test
```

## 📁 Project Structure

```
linux-manager-for-windows/
├── LinuxManager/          # Main WinUI 3 application
│   ├── Views/             # XAML UI pages and controls
│   ├── ViewModels/        # MVVM ViewModel layer
│   ├── Services/          # WSL operations and business logic
│   └── Contracts/         # Interface definitions
├── LinuxManager.Core/     # Shared core library
├── LinuxManager.Tests/    # xUnit test suite
└── LinuxManager.sln
```

## 🤝 Contributing

Contributions are welcome!

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit your changes: `git commit -m 'Add your feature'`
4. Push to the branch: `git push origin feature/your-feature`
5. Open a Pull Request

Please follow the existing MVVM architecture and add tests for new features.

## 🐛 Issues & Feedback

Found a bug or have a feature request? [Open an issue](https://github.com/NathanQuellec/linux-manager-for-windows/issues).

When reporting bugs, please include:
- Windows version
- WSL version (`wsl --version`)
- Steps to reproduce
- Expected vs. actual behavior

## 📝 License

This project is licensed under the MIT License — see [LICENSE.txt](LICENSE.txt) for details.

---

**Made with ❤️ by [Nathan Quellec](https://github.com/NathanQuellec)**
