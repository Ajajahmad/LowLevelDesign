Requirement: 
✅ Supported Log Levels
INFO
WARNING
ERROR
(DEBUG optional — we’ll include it for extensibility)

✅ Log Destinations (Targets)
Console
File
(DB excluded for now — adds infra complexity)

✅ Log Content
Each log entry contains:
timestamp
level
message
logger name (optional but good design)

✅ Configuration
User can configure:
minimum log level (threshold)
output destination(s)
Config provided at startup (no dynamic reload)

✅ Performance Scope
Synchronous logging only
No batching / async queue (out of scope)

✅ Out of Scope
Log rotation
Distributed logging
Remote streaming
Structured JSON logs
Thread safety


ENTITIES : 
LogLevel (enum) - INFOR, WARNING,ERROR
Logger - check level threshold , , create log event, send to destinations -- Facade/orchestrator role.
LogMessage (or LogEvent) -- timestamp, message ,level , loggername
LogDestination (interface)- consoleDestination, FileDestination
LoggerConfig--- minLevel, destinations ---- Avoid putting config inside Logger directly → SRP.

