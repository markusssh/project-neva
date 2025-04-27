extends Control

@export var showcase: GridContainer

func _ready() -> void:
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
