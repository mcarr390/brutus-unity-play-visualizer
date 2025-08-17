Brutus Planner

Goal-Oriented Action Planning for emergent, resource-driven simulations in Unity.

Summary

Brutus Planner is a GOAP-style planner designed for quantitative world state and cost-aware decisions in large, spatial simulations. Instead of boolean facts like HasWood = true, Brutus uses numeric goals and effects (e.g., Wood >= 10, action effect Wood += 1). This enables agents to repeat actions as needed and to weigh travel time, labor time, and tool/skill modifiers when choosing actions.

Brutus also supports plan sequencing that adapts to prior choices. Example: an early Mount Horse action increases movement speed, thereby changing the optimal order of subsequent tasks across the map. Similarly, tool upgrades can increase production rate and shift later action choices.

Key Features

Numeric state & goals: quantify needs (e.g., Food >= 20) and support incremental effects.

Cost model: separates travel time (dominant) and labor time, with optional multipliers from mounts/tools.

Dynamic re-planning: actions that change movement speed or production rates update costs for later steps.

Hierarchical planning: compose planners (coarse goals → decomposed subgoals) to control branching.

Location-aware: every action binds to a spatial location; plans minimize global cost, not just local steps.

Pluggable policies: heuristics, tie-breakers, and action selection strategies are swappable.

Visualization-ready: ships with a simple grid/agent visualizer for stepping through a plan.

Technologies & Patterns

Unity asmdefs & modularization

Brutus.Abstractions — interfaces/DTOs only (no Unity runtime types if possible)

Brutus.Runtime — planner core + default implementations

Brutus.Editor — authoring tools (optional)

Brutus.Visualization — sample grid visualizer (optional)

Dependency Inversion (abstractions assembly)

Consumers (A) reference only Abstractions.

Providers (B) implement interfaces behind those abstractions.

Unity-friendly DI / Composition

ScriptableObject Provider/Installer pattern to pick implementations without interface serialization.

Optional: VContainer/Extenject for constructor injection & lifetimes.

Data Authoring

ScriptableObjects for Action definitions, Locations, Tools/Modifiers.

Optional [SerializeReference] for polymorphic effects (with custom drawers).

Algorithms

GOAP with quantitative state: A*/WA* on a hybrid search space (resource deltas + spatial costs).

Heuristics: admissible lower bounds combining resource deficit and travel lower bounds (Manhattan or navmesh).

Hierarchical task networks (HTN-style) to constrain branching.

Testing

NUnit + Unity Test Framework; deterministic seeds for reproducible plans.

Micro-benchmarks with Unity Performance Testing API.

Concepts
Quantitative State

State variables: integers/floats (Wood, Food, IronIngot, Energy, etc.).

Preconditions: inequalities (e.g., Energy >= 5, ToolTier >= 2).

Effects: additive/multiplicative deltas (Wood += 1, MoveSpeed *= 2).

Costs

Travel cost: function of map distance / nav cost; affected by movement modifiers (mounts, roads).

Labor cost: action duration possibly reduced by tools/skills.

Total plan cost: sum of per-step travel + labor; planner chooses sequence that minimizes global cost.

Action-conditioned Modifiers

Early actions can reshape the cost landscape:

Mount Horse → MoveSpeed *= 2

Craft Better Axe → ChopLaborTime *= 0.6

Learn Skill → unlocks actions or increases yields

Architecture
Brutus.Abstractions   (interfaces, DTOs)
▲           ▲
│           │
Brutus.Runtime   YourGame.Modules (implementations, adapters)
▲           ▲
└─────── Composition (SO Installers / bootstrap)
▲
Brutus.Visualization (sample)


Abstractions: IAgent, IPlanner, IWorldState, IAction, ICostModel, simple DTOs.

Runtime: default planner, heuristic, and cost model implementations.

Composition: ScriptableObject installers select implementations; no direct coupling in consumers.

Visualization: optional sample scene to render grids, locations, and agent paths.

asmdef Reference Rules

Brutus.Abstractions: no references; Auto Referenced = off.

Brutus.Runtime: references Brutus.Abstractions.

Brutus.Visualization: references Brutus.Abstractions (+ your chosen runtime via composition).

Your game code depends on Abstractions; the Installer depends on both Abstractions and chosen Runtime.

Dependency Injection (Unity-native)

ScriptableObject Provider Pattern

Create an AgentProvider : ScriptableObject in Abstractions that returns IAgent.

Each implementation (in Runtime or your own module) provides a …Provider asset.

The visualizer or your game scene has a serialized field for AgentProvider and calls CreateAgent().

Swap implementations by swapping the asset — no code changes in consumers.

Why this works under the hood

Unity serializes references to assets (ScriptableObjects) reliably.

Consumers never serialize interfaces; they ask a provider to create the interface at runtime.

This sidesteps Unity’s interface serialization limitations while preserving strong decoupling.

Data Model

Action Definition

Id, DisplayName, LocationId

Preconditions: (Variable, Operator, Value)

Effects: ΔVariable, Multiplier, Unlocks

BaseLaborTime, BaseTravelMode (optional)

Location

Id, world/grid position, tags (e.g., Market, Forest, Blacksmith)

Modifiers

Movement (MoveSpeed * k), Yield (Yield * k), Time (LaborTime * k)

Author actions/locations as ScriptableObjects or JSON; adapters translate into Abstractions.

Planning Algorithm (under the hood)

Search: A* over states that include:

quantitative variables (compact vector) + current position + active modifiers

Heuristic:

Lower bound for remaining resource deficits (e.g., units still needed × best-case yield/time)

lower bound for travel (e.g., sum of Manhattan distances to a Steiner-tree approximation or min-spanning star)

Branching control:

Hierarchical planner delegates to sub-planners for categories (Gather, Process, Trade).

Pruning via dominance checks (discard states strictly worse in both cost and resources).

Quick Start

Install package (UPM Git URL or local).

Open sample scene in Brutus.Visualization.

Assign an AgentProvider asset to the Planner Visualizer.

Hit Play to see a plan like:

Plan found with cost 7
1: SkinAnimal
2: ChopWood
3: SmeltIron
4: ShapeBlade
5: SharpenBlade
6: CraftSword
7: SellToMerchantA


Move/rename locations or swap providers to see different plan choices.

Configuration

Cost Weights: tune travel vs labor importance.

Heuristic Mode: choose admissible or weighted (WA*) for faster convergence.

Modifiers: define mounts/tools and their multipliers.

Replanning Policy: on failure or environment change, re-evaluate from current state.

Extending

Add a new CostModel: implement ICostModel and bind via Installer.

Add a new Heuristic: implement IHeuristic.

Add new Action types: extend effects library (additive/multiplicative/custom delegates).

Swap Pathfinding: plug in NavMesh or grid A* under ITravelEstimator.

Testing

Unit tests for action legality, effect application, and heuristic admissibility.

Golden-file tests for plans in fixed seeds.

Performance tests: measure node expansions vs. target thresholds.

Design Trade-offs

Numeric state increases branching; mitigated via HTN and heuristics.

Action-conditioned modifiers (e.g., mounts) make cost non-stationary; heuristics must remain safe or consciously weighted.

SO-based composition favors editor ergonomics over pure code injection; containers are optional.

Roadmap

Editor authoring UI for actions/goals (graph view).

Built-in NavMesh travel estimator.

Save/restore planner state for long-running agents.

Multi-agent contention (shared resources/queues).

Example integrations (economy sim, colony sim).

Contributing

PRs welcome. Keep Abstractions stable and minimal.

Add tests for new heuristics/cost models.

Document any new Installer/Provider in the README’s DI section.

License

MIT (provisional; choose what fits your project).

Credits

Brutus Planner is inspired by classic GOAP/HTN ideas but tailored for resource-first, cost-aware, location-sensitive gameplay in Unity.