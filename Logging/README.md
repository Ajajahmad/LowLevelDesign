# Simple Logger System

A lightweight, synchronous logging framework designed with SOLID principles.

---

## 📋 Requirements

### ✅ Supported Log Levels
- [x] **INFO**
- [x] **WARNING**
- [x] **ERROR**
- [x] **DEBUG** (Optional — included for extensibility)

### ✅ Log Destinations (Targets)
- [x] **Console**
- [x] **File**
- [ ] *DB excluded for now (infrastructure complexity)*

### ✅ Log Content
Each log entry includes:
- **Timestamp**: When the event occurred.
- **Level**: The severity of the log.
- **Message**: The descriptive text.
- **Logger Name**: (Optional) For identifying the source class/module.

### ✅ Configuration
- **Minimum Log Level**: Define a threshold (e.g., only log ERROR and above).
- **Output Destinations**: Configure one or many targets.
- **Startup Config**: Parameters are provided at initialization (no dynamic reloading).

---

## 🚫 Out of Scope
To maintain simplicity, the following features are explicitly excluded:
* Log rotation and retention policies.
* Distributed or remote streaming.
* Structured JSON logging.
* Thread safety and asynchronous batching.

---

## 🏗️ System Architecture

### Core Entities
| Entity | Role | Pattern |
| :--- | :--- | :--- |
| **LogLevel** | Defines severity (INFO, WARNING, ERROR) | Enum |
| **Logger** | Orchestrates flow and checks thresholds | Facade / Singleton |
| **LogMessage** | Encapsulates all data for a single log entry | Value Object |
| **LogDestination** | Abstract interface for output targets | Strategy |
| **LoggerConfig** | Decouples settings from logic (SRP) | Config Holder |



### Final Design Structure
* **`LogLevel`** (enum)
* **`LogMessage`** (value object)
* **`ILogDestination`** (strategy)
    * `ConsoleDestination`
    * `FileDestination`
* **`IFormatter`** (strategy)
    * `SimpleFormatter`
* **`LoggerConfig`** (config holder)
* **`Logger`** (Singleton + Facade)
* **`Program`** (Demo implementation)
