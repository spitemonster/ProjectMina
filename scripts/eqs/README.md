# Environment Query System

Inspired by the system of the same name included with Unreal Engine. Built to hook into the Behavior Tree system and provide an easy way to query and score points in the environment.

Implemented the Godot way by using Nodes for building queries. There is a "Run Environment Query" Action Node provided with the Behavior Tree. Its first child node should be a Query node, the Query node's first child a Context, and the Context's children should all be tests. When the "Run Environment Query" task is called, each test is run on each point in the given context.

At time of writing, the EQS uses a ticketing system to moderate performance; a Query submits a request to be processed that is handled by the main EQS class. The EQS processes requests sequentially as they are received, one per physics tick. Queries are run on the physics tick to allow for safe access to PhysicsDirectSpaceState3D.

## Core

### EQS

Core Node; this should be loaded as an autoload in your game world.

### Query

### Context

### Tests

## Performance

At time of writing I have done zero performance testing. My anecdotal report early on is that while working on my 2019 Macbook Pro, the only frametime shenanigans I experience occur when Debug is enabled on EQS nodes which result in a lot of debug spheres being drawn. Future performance testing to come.