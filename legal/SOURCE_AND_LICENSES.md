# OmniPlayer bundled component notice

OmniPlayer is a launcher/orchestrator. The portable package aggregates unmodified upstream binaries; each component remains under its own license.

| Component | Version in this package | License | Corresponding source |
| --- | --- | --- | --- |
| mpv-omniphony | v0.4.1-2 / mpv v0.41.0 | GPL-3.0-or-later combined distribution | https://github.com/mgth/mpv-omniphony/tree/v0.4.1-2 |
| Omniphony / liborender / Studio | v0.3.3 installed runtime | GPL-3.0-or-later | https://github.com/mgth/Omniphony/tree/v0.3.3 |
| harletty-bridge | v0.7.1 | Apache-2.0; vendored decoder components retain their notices | https://github.com/harletty/harletty-bridge/tree/v0.7.1 |

The mpv runtime directory includes third-party multimedia libraries. See `MPV-THIRD-PARTY-NOTICES.md` and the upstream repositories for their individual terms. This package does not include Dante Virtual Soundcard; DVS is selected as an external system audio driver.

OmniPlayer does not remove or replace upstream copyright notices. If this package is redistributed, keep this `legal` directory with it.
