extends Node

static var Players = []

func _input(event):
  if event.is_action_released("host"):
    NetworkManager.host()

  if event.is_action_released("join"):
    NetworkManager.join()