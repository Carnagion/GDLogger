extends Node

func _init() -> void:
	self.set_file_path(DEFAULT_FILE_PATH)

const DEFAULT_FILE_PATH: String = "user://Log.txt"

const MAX_ENTRY_COUNT: int = 100

const MAX_FLUSH_INTERVAL_SECONDS: int = 60

const MAX_FLUSH_INTERVAL_MESSAGES: int = 10

var _entries: Array = []

var _file: File = File.new()

var _last_synced: int = 0

var file_path: String setget set_file_path, get_file_path

func _notification(notif: int) -> void:
	if notif == MainLoop.NOTIFICATION_WM_QUIT_REQUEST and self._file.is_open():
		self._file.close()

func set_file_path(path: String) -> void:
	# If a previous log file already exists, save its contents and close it
	if self._file.is_open():
		self._flush(true)
		self._file.close()
	self._file.open(path, File.WRITE)

func get_file_path() -> String:
	return self._file.get_path_absolute()

func write(entry: Entry) -> void:
	print_debug(entry)
	self._file.store_line(entry.to_string())
	if self._entries.size() == Log.MAX_ENTRY_COUNT:
		self._entries.pop_front().free()
	self._entries.push_back(entry)
	self._flush()

func notify(message: String) -> void:
	self.write(Entry.new(message, Entry.MessageSeverity.NOTIFICATION))

func warning(message: String) -> void:
	self.write(Entry.new(message, Entry.MessageSeverity.WARNING))

func error(message: String) -> void:
	self.write(Entry.new(message, Entry.MessageSeverity.ERROR))

func _flush(force: bool = false) -> void:
	var now: int = OS.get_ticks_msec() / 1000
	if (not force) and (now - self._last_synced < Log.MAX_FLUSH_INTERVAL_SECONDS) and (self._entries.size() < Log.MAX_FLUSH_INTERVAL_MESSAGES):
		return
	# If it has been 60 seconds since the last flush, or if there are 10 or more entries in the queue, flush immediately
	self._entries.clear()
	self._file.flush()
	self._last_synced = now

class Entry extends Object:

	func _init(message: String, severity: int) -> void:
		self._message = message
		self._severity = severity
		self._timestamp = OS.get_datetime()

	var _message: String setget , get_message

	var _timestamp: Dictionary setget , get_timestamp

	var _severity: int setget , get_severity

	func get_message() -> String:
		return _message # Using self here will trigger the getter and recursively run the function. Dumb design decision, Godot devs

	func get_timestamp() -> Dictionary:
		return _timestamp.duplicate() # Using self here will trigger the getter and recursively run the function

	func get_severity() -> int:
		return _severity # Using self here will trigger the getter and recursively run the function

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

	enum MessageSeverity {
		NOTIFICATION,
		WARNING,
		ERROR,
	}
