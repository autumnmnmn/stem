# stem

Stem is an ECS (Entity Component System) in C#, & an OpenGL rendering engine. Mostly written back in 2020 or 2021, I think. This was mostly an exercise in learning OpenGL.

The Sandbox package contains a few example modules putting it to use. Sandbox.Launcher is a CLI for launching those modules.

## What's an ECS?

From Wikipedia:
"""
An ECS comprises entities composed from components of data, with systems which operate on the components.

ECS follows the principle of composition over inheritance, meaning that every entity is defined not by a type hierarchy, but by the components that are associated with it. Systems act globally over all entities which have the required components.
"""

### ECS in stem

A StemInstance is an ECS. Its entities are represented by integer IDs. These IDs are provisioned by its EntityManager, which associates each entity with any Aspects that are assigned to or revoked from it. An Aspect is just any struct that has been registered with the instance to be used as an aspect. These "aspects" are what would be called "components" in most ECS implementations. In addition to its entities and their aspects, a stem instance has a RuleCanon consisting of an ordered list of RuleBooks, which in turn consist of ordered lists of Rules. Each Rule operates once per tick on all entities which, at the time of its execution, have a particular combination of aspects. In other ECS implementations such "rules" would be called "systems". I just like this naming scheme better, I don't have a fantastic justification for disregarding convention.

## Usage

TODO.

For now just look at the AntSim module in Sandbox, that's the best example so far.

