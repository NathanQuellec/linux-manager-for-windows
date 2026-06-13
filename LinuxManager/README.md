# WSLStudio

A modern, feature-rich desktop application for managing Windows Subsystem for Linux (WSL) distributions on Windows. Built with .NET 6+ and WinUI 3.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Release](https://img.shields.io/github/v/release/NathanQuellec/WSLStudio)](https://github.com/NathanQuellec/WSLStudio/releases)
[![Build Status](https://github.com/NathanQuellec/WSLStudio/actions/workflows/build-test.yml/badge.svg)](https://github.com/NathanQuellec/WSLStudio/actions)

## Overview

WSLStudio simplifies the management of Windows Subsystem for Linux distributions. It provides an intuitive GUI for common WSL operations, eliminating the need for command-line interactions.

## ✨ Features

- **Distribution Management**: Install, uninstall, and manage multiple WSL distributions
- **Quick Operations**: Start, stop, and restart distributions with a single click
- **System Integration**: Seamlessly integrate with Windows file explorer and terminal
- **Modern UI**: Built with WinUI 3 for a native, fluent design experience
- **Cross-Distribution Support**: Works with WSL1 and WSL2 distributions
- **Robust Error Handling**: Clear error messages and recovery options
- **Lightweight**: Minimal resource consumption

## 🔧 Requirements

- **Windows 10** (Build 19041 or later) or **Windows 11**
- **.NET 6 Runtime** or later
- **WSL** enabled on your system ([Installation Guide](https://docs.microsoft.com/en-us/windows/wsl/install))

## 📥 Installation

### Option 1: Pre-built Release (Recommended)

1. Download the latest release from [GitHub Releases](https://github.com/NathanQuellec/WSLStudio/releases)
2. Extract the zip file to your desired location
3. Run `WSLStudio.exe`

### Option 2: Clone and Build from Source

See [Building from Source](#building-from-source) below.

## 🚀 Quick Start

1. **Launch WSLStudio**
   - Run the executable or create a shortcut for quick access

2. **View Your Distributions**
   - The main window displays all installed WSL distributions
   - See distribution details including version and state

3. **Manage Distributions**
   - Click on a distribution to see available actions:
     - **Start/Stop**: Control the distribution state
     - **Launch Terminal**: Open a terminal in the distribution
     - **Remove**: Uninstall the distribution (with confirmation)

## 🏗️ Building from Source

### Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/) (Community or higher) with:
  - .NET 6 SDK or later
  - Windows App SDK workload
- Or [Visual Studio Code](https://code.visualstudio.com/) with C# extension

### Build Steps

1. **Clone the Repository**
   ```bash
   git clone https://github.com/NathanQuellec/WSLStudio.git
   cd WSLStudio
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the Project**
   ```bash
   dotnet build
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```

5. **Create a Release Build**
   ```bash
   dotnet publish -c Release
   ```

## 🧪 Testing

The project includes unit tests using xUnit. Run tests with:

```bash
dotnet test
```

For detailed test output:

```bash
dotnet test --verbosity detailed
```

## 📁 Project Structure

```
WSLStudio/
├── LinuxManager/              # Main application
│   ├── Views/                 # XAML UI components
│   ├── ViewModels/            # MVVM ViewModel layer
│   ├── Services/              # Business logic & WSL operations
│   ├── Contracts/             # Interface definitions
│   └── Resources/             # Assets and localization
├── LinuxManager.Tests/        # Unit test suite
├── LinuxManager.Core/         # Core library shared logic
└── README.md
```

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

1. **Fork the repository** on GitHub
2. **Create a feature branch**: `git checkout -b feature/your-feature`
3. **Commit your changes**: `git commit -am 'Add your feature'`
4. **Push to the branch**: `git push origin feature/your-feature`
5. **Open a Pull Request** with a clear description of your changes

### Development Guidelines

- Follow the existing code style and architecture (MVVM pattern)
- Add unit tests for new features
- Update documentation as needed
- Ensure builds pass before submitting PR

## 🐛 Issues & Feedback

Found a bug or have a feature request? [Open an issue](https://github.com/NathanQuellec/WSLStudio/issues) on GitHub.

When reporting bugs, please include:
- Windows version
- WSL version (check with `wsl --version`)
- Steps to reproduce
- Expected vs. actual behavior
- Screenshots if applicable

## 📝 License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [Windows App SDK](https://github.com/microsoft/WindowsAppSDK) and [WinUI 3](https://github.com/microsoft/microsoft-ui-xaml)
- Uses [Windows Community Toolkit](https://github.com/windows-toolkit/WindowsCommunityToolkit)
- Inspired by the WSL community

## 📚 Resources

- [WSL Documentation](https://docs.microsoft.com/en-us/windows/wsl/)
- [WinUI 3 Documentation](https://docs.microsoft.com/en-us/windows/apps/winui/winui3/)
- [Windows App SDK](https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/)

---

**Made with ❤️ by [Nathan Quellec](https://github.com/NathanQuellec)**
