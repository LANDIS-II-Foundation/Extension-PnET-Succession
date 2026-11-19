# What is the PnET-Succession Extension?

The PnET-Succession extension implements succession with cohorts defined by age ranges and including biomass per cohort. Most parameters can change over time, due to climate change for example.

The PnET-Succession extension was originally based on the Biomass Succession extension of Scheller and Mladenoff (2004), embedding elements of the PnET-II ecophysiology model of Aber et al (1995) to simulate growth as a competition for available light and water, replacing the existing competition for “growing space” algorithms (De Bruijn et al. 2014). PnET (Photosynthesis and EvapoTranspiration) is a simple, lumped parameter model of carbon and water balances of forests (Aber and Federer 1992), built on two principal relationships: 1) maximum photo-synthetic rate is a function of foliar nitrogen concentration, and 2) stomatal conductance is a function of realized photosynthetic rate. PnET-Succession uses one PnET simulation for each tree species-age cohort and uses water and light consumption by these cohorts to implement competition. PnET-Succession calculates resources in terms of Non-Structural Carbon (NSC) for each cohort in its PnET part and assumes that cohorts die when their NSC level drops below a threshold. The PnET-Succession extension also tracks dead biomass over time, divided into two pools: woody and leaf litter.

# Release Notes

- Latest official release: Version 6.1.3 — November 2026
  - Update to UCL v2.0.2 and PnET Cohort Library v2.4.2
- This version has functionality identical to v5.1, with documentation below. The new release is simply for compatibility with LANDIS-II v.8. This also means that calibrations performed for v5.1 should apply to simulations using this version.

# Requirements

To use PnET-Succession, you need:

- The [LANDIS-II model v8.0](http://www.landis-ii.org/install) installed on your computer.


# Download

Version 6.1.2 installer can be downloaded here: [PnET Succession v6.1.2 Installer](<https://github.com/LANDIS-II-Foundation/Extension-PnET-Succession/releases/download/v6.1.2/LANDIS-II-V8_PnET-Succession_6.1.2_setup.exe>). To install it on your computer, launch the installer.

# Example Files

LANDIS-II requires a global parameter file for your scenario, and separate parameter files for each extension.

Example files using PnET-Succession are here: [Examples](https://github.com/LANDIS-II-Foundation/Extension-PnET-Succession/tree/master/deploy/examples/biomass-Pnet-succession-example-v8).

# Citation

Gustafson, Eric J., Brian R. Sturtevant, Brian R. Miranda, Zaixing Zhou.  2023.  PnET-Succession v 6.0.3:  Comprehensive description of an ecophysiological succession extension within the LANDIS-II forest landscape model.  Published by the LANDIS-II Foundation.  URL: [https://github.com/LANDIS-II-Foundation/Foundation-Publications/blob/main/Description of PnET-Succession v5.1.pdf](https://github.com/LANDIS-II-Foundation/Foundation-Publications/blob/main/Description%20of%20PnET-Succession%20v5.1.pdf).

# Support

If you have a question, please contact Brian Sturtevant. You can also ask for help in the [LANDIS-II users group](http://www.landis-ii.org/users).

If you come across any issue or suspected bug, please post about it in the [issue section of the Github repository](https://github.com/LANDIS-II-Foundation/Extension-PnET-Succession/issues) (GitID required).

# Authors

Arjan de Bruijn: originally Purdue University; Eric Gustafson (emeritus), Brian Miranda, Brian Sturtevant:; US Forest Service, and Zaixing Zhou (University of New Hampshire)

Mail : brian.r.sturtevant@usda.gov

