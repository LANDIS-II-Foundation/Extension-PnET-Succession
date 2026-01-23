# What is the PnET-Succession Extension?

The PnET-Succession extension implements succession with cohorts defined by age ranges and including biomass per cohort. Most parameters can change over time, due to climate change for example.

The PnET-Succession extension was originally based on the Biomass Succession extension of Scheller and Mladenoff (2004), embedding elements of the PnET-II ecophysiology model of Aber et al (1995) to simulate growth as a competition for available light and water, replacing the existing competition for “growing space” algorithms (De Bruijn et al. 2014). PnET (Photosynthesis and EvapoTranspiration) is a simple, lumped parameter model of carbon and water balances of forests (Aber and Federer 1992), built on two principal relationships: 1) maximum photo-synthetic rate is a function of foliar nitrogen concentration, and 2) stomatal conductance is a function of realized photosynthetic rate. PnET-Succession uses one PnET simulation for each tree species-age cohort and uses water and light consumption by these cohorts to implement competition. PnET-Succession calculates resources in terms of Non-Structural Carbon (NSC) for each cohort in its PnET part and assumes that cohorts die when their NSC level drops below a threshold. The PnET-Succession extension also tracks dead biomass over time, divided into two pools: woody and leaf litter.

# Release Notes

- Latest official release: Version 6.0.3 — January 2026
- This version has functionality identical to v5.1, with documentation below. The new release is simply for compatibility with LANDIS-II v.8.  This also means that calibrations performed for v5.1 should apply to simulations using this version.
- Download the User Guide here: [PnET-Succession v6.0 User Guide](https://github.com/LANDIS-II-Foundation/Extension-PnET-Succession/blob/master/deploy/docs/LANDIS-II%20PnET-Succession%20v6.0%20User%20Guide%20Jan21%202026.pdf).
- Download the Climate Library guide here: [User guide for Climate Library](https://github.com/LANDIS-II-Foundation/Library-Climate/blob/v8.0/docs/LANDIS-II%20Climate%20Library%20v5.0%20User%20Guide.pdf)
- Full release details can be found in the User Guide and on [GitHub](https://github.com/LANDIS-II-Foundation/Extension-PnET-Succession).
- A detailed description of the model can be found in this updated document: [Description of PnET-Succession v5.1.pdf](https://github.com/LANDIS-II-Foundation/Foundation-Publications/blob/main/Description%20of%20PnET-Succession%20v5.1.pdf)
- A spreadsheet that demonstrates some of the specific model functions is available to explore the impacts of certain parameter choices: [PnET-Succession function worksheet v6.0.xlsx](https://github.com/LANDIS-II-Foundation/Extension-PnET-Succession/blob/master/deploy/docs/PnET-Succession%20function%20worksheet%20v6.0.xlsx)
- This version is compatible with all officially released extensions – unless a restriction to a specific extension is otherwise identified. 

# Requirements

To use PnET-Succession, you need:

- The [LANDIS-II model v8.0](http://www.landis-ii.org/install) installed on your computer.


# Download

Version 6.0.3 installer can be downloaded here:  [PnET-Succession v6.0.3 Installer](https://github.com/LANDIS-II-Foundation/Extension-PnET-Succession/blob/master/deploy/installer/LANDIS-II-V8%20PnET-Succession%206.0.3-setup.exe). To install it on your computer, launch the installer.

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
