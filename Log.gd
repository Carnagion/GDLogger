## Handles the logging of messages to a log file.
extends Node

## Initialises a new [Log].
func _init() -> void:
	self.set_file_path(self.DEFAULT_FILE_PATH)

const DEFAULT_FILE_PATH: String = "user://Log.txt"

const MAX_ENTRY_COUNT: int = 100

const MAX_FLUSH_INTERVAL_SECONDS: int = 60

const MAX_FLUSH_INTERVAL_MESSAGES: int = 10

var _entries: Array = []

var _file: File = File.new()

var _last_synced: int = 0

## The file path to which log entries are written.
var file_path: String setget set_file_path, get_file_path

## Emitted when an entry is written to the log file.
signal entry_written(entry)

## Closes the log file if the quit request notification is received.
func _notification(notif: int) -> void:
	if notif == MainLoop.NOTIFICATION_WM_QUIT_REQUEST and self._file.is_open():
		self._file.close()

## Sets the file path to which log entries are written, automatically closing the previous log file (if any).
func set_file_path(path: String) -> void:
	# If a previous log file already exists, save its contents and close it
	if self._file.is_open():
		self._flush(true)
		self._file.close()
	self._file.open(path, File.WRITE)

## Gets the file path to which log entries are written.
func get_file_path() -> String:
	return self._file.get_path_absolute()

## Writes the text representation of the [Entry] to the log file. Also writes to the console in debug mode.
func write_entry(entry: Entry) -> void:
	print_debug(entry)
	self._file.store_line(entry.to_string())
	if self._entries.size() == self.MAX_ENTRY_COUNT:
		self._entries.pop_front().free()
	self._entries.push_back(entry)
	self._flush()
	self.emit_signal("entry_written", entry)

## Writes the message to the log file, encoding it as a notification.
func write(message: String) -> void:
	self.write_entry(Entry.new(message, Entry.MessageSeverity.NOTIFICATION))

## Writes the message to the log file, encoding it as a warning.
func warning(message: String) -> void:
	self.write_entry(Entry.new(message, Entry.MessageSeverity.WARNING))

## Writes the message to the log file, encoding it as an error.
func error(message: String) -> void:
	self.write_entry(Entry.new(message, Entry.MessageSeverity.ERROR))

func _flush(force: bool = false) -> void:
	var now: int = OS.get_ticks_msec() / 1000
	if (not force) and (now - self._last_synced < self.MAX_FLUSH_INTERVAL_SECONDS) and (self._entries.size() < self.MAX_FLUSH_INTERVAL_MESSAGES):
		return
	# If it has been 60 seconds since the last flush, or if there are 10 or more entries in the queue, flush immediately
	self._entries.clear()
	self._file.flush()
	self._last_synced = now

## Represents a log entry.
class Entry extends Object:

	## Initialises a new [Entry] with the specified parameters.
	func _init(message: String, severity: int) -> void:
		self.message = message
		self.severity = severity
		self.timestamp = OS.get_datetime()

	## The message of the [Entry].
	var message: String setget , get_message

	## The time when the [Entry] was created, returned as a [Dictionary] of keys (see [method OS.get_datetime]).
	var timestamp: Dictionary setget , get_timestamp

	## The severity level of the [Entry].
	var severity: int setget , get_severity

	## Gets the message of the [Entry].
	func get_message() -> String:
		return message # Using self here will trigger the getter and recursively run the function. Dumb design decision, Godot devs

	## Gets the timestamp of the [Entry].
	func get_timestamp() -> Dictionary:
		return timestamp.duplicate() # Using self here will trigger the getter and recursively run the function

	## Gets the severity level of the [Entry].
	func get_severity() -> int:
		return severity # Using self here will trigger the getter and recursively run the function

	## Returns a [String] that represents the [Entry].
	func _to_string() -> String:
		return "[%s] at %d:%d:%d - %s" % [Entry._severity_to_string(self._severity), self._timestamp["hour"], self._timestamp["minute"], self._timestamp["second"], self._message]

	static func _severity_to_string(severity: int) -> String:
		match severity:
			MessageSeverity.NOTIFICATION:
				return "Notification"
			MessageSeverity.WARNING:
				return "Warning"
			MessageSeverity.ERROR:
				return "Error"
			_: # Technically, this case will never be reached, but it needs to be there since all paths must return a value
				return ""

	## Represents a log entry severity.
	enum MessageSeverity {
		NOTIFICATION, ## Miscellaneous information.
		WARNING, ## Minor errors that can usually be recovered from.
		ERROR, ## Major errors that usually stop the program.
	}
