extends Node

const DB_PATH: String = "res://Main/Database/db_content/db_content.db"
const AUTHORS_TABLE: String = "authors"
const PAINTINGS_TABLE: String = "paintings"
const ANY_CONDITION: String = "true"
const ALL_COLUMNS: Array[String] = ["*"]
const NET_EXPORT_FILE: String = "res://Main/NetworkingArchitecture/NetExportData.json"

var db: SQLite = SQLite.new()

func _enter_tree() -> void:
	db.path = DB_PATH
	db.open_db()

func get_authors() -> Array:
	return db.select_rows(
		AUTHORS_TABLE, 
		ANY_CONDITION, 
		["id", "name"]
	)

func get_paintings_by_author(author_id: int) -> Array:
	return db.select_rows(
		PAINTINGS_TABLE, 
		"author_id=" + str(author_id), 
		["id", "name"]
	)

func get_painting_by_id(id: int) -> Dictionary:
	return db.select_rows(
		PAINTINGS_TABLE,
		"id=" + str(id),
		ALL_COLUMNS
	)[0]

func get_author_by_id(id: int) -> Dictionary:
	return db.select_rows(
		AUTHORS_TABLE,
		"id=" + str(id),
		ALL_COLUMNS
	)[0]

func _exit_tree() -> void:
	db.close_db()

func _ready() -> void:
	if OS.has_feature("debug") and OS.has_feature("dedicated_server"):
		var painting_ids: Array = db.select_rows(
			PAINTINGS_TABLE,
			ANY_CONDITION,
			["id"]
		).map(func(entry): return entry.get("id"))
		var save_file = FileAccess.open(NET_EXPORT_FILE, FileAccess.WRITE)
		if save_file != null:
			save_file.store_string(JSON.stringify(painting_ids))
			save_file.close()
			print("Net data exported successfully (" + NET_EXPORT_FILE + ")")
		else:
			printerr("Coudn't write to NET_EXPORT_FILE: " + NET_EXPORT_FILE)
