extends Control

@export var showcase: GridContainer
@export var progress_container: PanelContainer
@export var replay_await_timer: Timer
@export var replay_await_progress: ProgressBar

var replay_await_time = MultiplayerController.Client_ReplayAwaitTime
var players_ready: int = 0
var out_of: int = MultiplayerController.Client_MaxPlayers

func _ready() -> void:
	%PlayAgainButton.text = "Заново (" + str(0) + "/"\
	 + str(MultiplayerController.Client_MaxPlayers) + ")"
	
	MultiplayerController.ReplayAwaitStatusChanged.connect(_on_replay_await_status_changed)
	
	var players: Dictionary = MultiplayerController.Client_Players
	var images: Dictionary = MultiplayerController.Client_FinalImages
	var scores: Dictionary = MultiplayerController.Client_Scores
	var player_cards: Array[PlayerCard]
	
	for player_id in images.keys():
		var p: PlayerCard = PlayerCard.new()
		p.player_name = players[player_id].PlayerName if players.has(player_id) \
			else "Disconnected"
		p.image = images[player_id]
		p.score = scores.get(player_id, 0)
		player_cards.append(p)
	
	player_cards.sort_custom(func(a, b): return a.score > b.score)
	
	var i: int = 0
	for pc in player_cards:
		i += 1
		var item: ShowcaseItem = ShowcaseItem.create(
			pc.image, 
			i,
			pc.player_name,
			pc.score)
		showcase.add_child(item)
	
	MultiplayerController.Client_NotifyNewSceneReady()

class PlayerCard:
	var image: Image
	var player_name: String
	var score: int

func _on_back_to_menu_button_pressed() -> void:
	Networking.LeaveGame()
	get_tree().change_scene_to_file("res://Main/Menu/Scenes/enter_lobby.tscn")

func _on_play_again_button_toggled(toggled_on: bool) -> void:
	MultiplayerController.Client_NotifyReplayStatusChange(toggled_on)

func _on_replay_await_status_changed(started: bool, players_ready: int, out_of: int):
	%PlayAgainButton.text = "Заново (" + str(players_ready) + "/" + str(out_of) + ")"
	if started:
		progress_container.show()
		replay_await_timer.start(replay_await_time)
	else:
		progress_container.hide()
		replay_await_timer.stop()

func _process(_delta: float) -> void:
	replay_await_progress.value = replay_await_timer.time_left / replay_await_time * 100
