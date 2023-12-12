class_name TrackedValue


var value:
	set(new_value):
		if _values.has(NetworkManager.current_tick) and _values[NetworkManager.current_tick].updated:
			return

		_values[NetworkManager.current_tick] = {
			'value': new_value,
			'updated': false
		}

		if NetworkManager.current_tick - _earliest_tick > floor(NetworkManager.MAX_MESSAGE_DELAY * NetworkManager.TICKS_PER_SECOND):
			var new_earliest_tick = NetworkManager.current_tick - floor(NetworkManager.MAX_MESSAGE_DELAY * NetworkManager.TICKS_PER_SECOND)

			for tick in range(_earliest_tick, new_earliest_tick):
				if _values.has(tick):
					_values.erase(tick)

			_earliest_tick = new_earliest_tick

	get:
		if not _values.has(NetworkManager.current_tick):
			return null

		return _values[NetworkManager.current_tick].value

var old_value:
	get:
		if not _values.has(NetworkManager.current_tick - 1):
			return null

		return _values[NetworkManager.current_tick - 1].value

var interpolated_value

var _interpolate_event
var _should_update_provider
var _values = {}
var _last_updated_tick

var _earliest_tick = 0


func _init(initial_value, interpolate_event = null, should_update_provider = null):
	_values[NetworkManager.current_tick] = {
		'value': initial_value,
		'updated': false
	}

	interpolated_value = initial_value

	_last_updated_tick = NetworkManager.current_tick

	_should_update_provider = should_update_provider

	_interpolate_event = interpolate_event


func _values_equal(a, b):
	if a is Dictionary and b is Dictionary:
		return a.hash() == b.hash()
		
	if a is Array and b is Array:
		return a.hash() == b.hash()

	return a == b


func _should_update() -> bool:
	if _should_update_provider == null:
		return not _values_equal(value, old_value)

	return _should_update_provider.call(value, old_value)


func _update_value(tick, new_value):
	_last_updated_tick = tick

	_values[tick] = {
		'value': new_value,
		'updated': true
	}

	NetworkManager._earliest_updated_tick = min(NetworkManager._earliest_updated_tick, tick)


func _set_updated():
	_values[NetworkManager.current_tick].updated = true


func _process(delta):
	if _interpolate_event == null:
		interpolated_value = value

		return

	interpolated_value = _interpolate_event.call(_values[NetworkManager.current_tick].value, interpolated_value, max(0, NetworkManager.current_tick - _last_updated_tick), delta)