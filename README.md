# AutoAcceptInvite

AutoAcceptInvite is a [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for 
[Resonite](https://resonite.com/) for automatically accepting invites or invite requests from trusted friends. 

## Installation

1. Install the mod using [Resolute](https://github.com/Gawdl3y/Resolute)

## Manual Installation

1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
2. Download the latest release from the [releases page](https://github.com/Restrainite/AutoAcceptInvite/releases).
3. Place the DLL file in your `rml_mods` folder.
4. (Optional) Install [ResoniteModSettings](https://github.com/badhaloninja/ResoniteModSettings) for an in-game configuration UI.

## How to use

### Enabling Auto-Accept for Friends

1. Open your contacts list in Resonite.
2. Navigate to a friends contact card.
3. Look for the **Auto-Accept Disabled/Enabled** button in the contacts action menu
4. Click the button to enable/disable auto-accept for that specific friend
    - When enabled, the mod will automatically accept invites and invite requests from that friend
    - The button is **disabled by default** for all friends for safety

### Configuration

You can customize the mods behavior through the mod settings using [ResoniteModSettings](https://github.com/badhaloninja/ResoniteModSettings):

- Control which types of invites are automatically accepted.
- Set your minimum online status level for auto-accepting.
- Configure a cooldown period between auto-accepted invites.

### Important Notes

- Auto-accept only works for friends you explicitly enable it for.
- You must be at or above your configured minimum online status level.
- There's a configurable cooldown between auto-accepted invites to prevent spam.

## Setting options

- **Auto-Accept Invites**: Automatically accept invites to other worlds. (Default: `true`)
- **Auto-Accept Invite Requests**: Automatically accept invite requests to your current instance. (Default: `true`)
- **Allow Forwarding to Instance Owner**: Allow forwarding invites to the instance owner. (Default: `true`)
- **Auto-Accept Forwarded Invite Requests**: Automatically accept forwarded invite requests. (Default: `true`)
- **Minimum Online Status Level**: Your minimum online status level required to auto-accept invites. (Default: `Online`)
- **Minimum Interval (Seconds)**: The minimum interval in seconds between auto-accepted invites. Other invites are ignored. (Default: `60`)

## About the project
AutoAcceptInvite is © 2025 by SnepDrone

Tested by Fuzy Sidwell and others

### License
AutoAcceptInvite is distributed under a [BSD 3-Clause License](https://github.com/Restrainite/AutoAcceptInvite?tab=BSD-3-Clause-1-ov-file).

### Contributing and Building from source
As an external contributor, when contributing to this project, please first discuss the change you wish to make via issue 
before making a pull request. 

Read more in our [GitHub repo](https://github.com/Restrainite/AutoAcceptInvite/blob/main/CONTRIBUTING.md).
