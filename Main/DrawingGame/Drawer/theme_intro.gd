extends Intro

func _init() -> void:
	super._init()
	MultiplayerController.RoundThemeReceived.connect(_on_round_theme_recieved)

func _on_round_theme_recieved(r_t: RoundTheme) -> void:
	%ThemeVBoxContainer.show()
	%HeaderLabel.text = r_t.Author
	%ThemeNameLabel.text = r_t.ThemeName
