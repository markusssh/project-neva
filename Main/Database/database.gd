extends Node

const DB_PATH: String = "res://Main/Database/db_content/db_content.db"
const AUTHORS_TABLE: String = "authors"
const PAINTINGS_TABLE: String = "paintings"
const ANY_CONDITION: String = "true"
const ALL_COLUMNS: Array[String] = ["*"]

var db: SQLite = SQLite.new()

func _enter_tree() -> void:
	db.path = DB_PATH
	db.open_db()

func get_authors_list() -> Array:
	return db.select_rows(AUTHORS_TABLE, ANY_CONDITION, ALL_COLUMNS)

func get_paintings_list(author_id: int) -> Array:
	return db.select_rows(
		PAINTINGS_TABLE, 
		"author_id=" + str(author_id), 
		["id", "name"]
	)

func _exit_tree() -> void:
	db.close_db()
