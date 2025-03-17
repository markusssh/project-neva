extends Control

@export var showcase: GridContainer
@export var curr_drawing: TextureRect
@export var drawing_display: SplitContainer
@export var stars_container: StarsContainer

func _ready() -> void:
	showcase.hide()
	var images: Dictionary
	
	for player in MultiplayerController.CurrentLobbyPlayers:
		images[player] = ImageHelper.CreateImageFromCompressed(
			MultiplayerController.CurrentLobbyPlayers[player].FinalImageData)
	
	showcase.columns = ceili(sqrt(images.size()))
	
	for player in images:
		var drawing_texure = ImageTexture.create_from_image(images[player])
		
		if Networking.IsMyPeer(player):
			var t: TextureRect = TextureRect.new()
			t.texture = drawing_texure
			showcase.add_child(t)
			continue
		
		curr_drawing.texture = drawing_texure
		
		run_timer(MultiplayerController.TimeToRateOneSec)
		showcase.add_child(curr_drawing.duplicate())
		var score := stars_container.get_score()
		MultiplayerController.SendScoreFromClient(player, score)
		stars_container.clear_score()
	
	run_timer(1)
	drawing_display.hide()
	showcase.show()

func run_timer(sec: int) -> void:
	var timer: Timer = Timer.new()
	timer.one_shot = true
	add_child(timer)
	timer.start(sec)
	await timer.timeout
	timer.queue_free()
