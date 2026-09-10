# Accusite Constellation Validator

**Version:** 1.0  
**Project:** ISRA_PS_AddOns — Accusite Epic  
**ADO Issue:** #93907  
**Team:** ISRA Vision CAD & Simulation, Atlas Copco  

## Overview

A Process Simulate add-on that validates whether the Accusite sensor holder
LED constellation is trackable from the external tracker at each measurement
point along a robotic path.

## Features

- **Multi-tracker support** — up to 4 trackers, first OK wins
- **Angle filter** — 55° max per Perceptron specification
- **FOV check** — tracker field-of-view validation
- **Line-of-sight check** — cylinder-based occlusion detection
- **Collision check** — via PS Collision Viewer pairs
- **4 Tracking planes** (A/B/C/D) — spatially distributed constellation criteria
- **On-demand visualization** — LED squares + camera lines per point click
- **Angle Details tab** — per-emitter angle values, collapsible per point

## Tracking Planes

| Plane | Type | Groups | Condition |
|-------|------|--------|-----------|
| A | Primary 1 | NAUO1/2/3/4 | NAUO3≥1 AND NAUO4≥1 AND (NAUO1≥1 OR NAUO2≥1) |
| B | Primary 2 | NAUO5/6/7/8 | NAUO5≥1 AND NAUO7≥1 AND (NAUO6≥1 OR NAUO8≥1) |
| C | Secondary 1 | NAUO2/4/7/8 | ≥3 groups each ≥1 |
| D | Secondary 2 | NAUO1/3/5/6 | ≥3 groups each ≥1 |

## Supported Sensor Holders

| Type ID | Description |
|---------|-------------|
| `perc_01-03944-10` | Perceptron 01-03944-10 (8 groups × 5 emitters = 40 LEDs) |

## Requirements

- Process Simulate 2408
- .NET Framework 4.8
- ISRA.Core, ISRA.Components, ISRA.Calculations (ISRA_PS_AddOns solution)

## Installation

1. Build in Release configuration
2. DLL output: `DotNetCommands\Accusite\ConstellationAddon\`
3. Register via CommandReg (see PS API Guide §3.1)
4. Restart Process Simulate

## Usage

See Help/About in the add-on, or PS API Developer Guide (ADO Wiki).

## Roadmap

- [ ] Auto sensor reorientation for NOK points (#93906)
- [ ] Excel result export (#93907)
- [ ] Support for additional sensor holder types
- [ ] Help/About diagram showing NAUO group plane assignments

## Source

Perceptron Planner Builder specification  
(Pavel Machacek / Vitek — emitter validation logic)