extends Node

func _ready():
    AIPerformanceMonitor.initialize_performance_counters();

func _physics_process(delta):
    AIPerformanceMonitor.update_performance_counters()