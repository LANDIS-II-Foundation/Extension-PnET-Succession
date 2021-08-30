# What is the PnET-Succession Extension?

The  PnET-Succession extension implements succession with cohorts defined by age ranges and including biomass per cohort. Most parameters are able to change over time, due to climate change for example.

The PnET-Succession extension is based on the Biomass Succession extension of Scheller and Mladenoff (2004), embedding elements of the PnET-II ecophysiology model of Aber et al (1995) to simulate growth as a competition for available light and water, replacing the existing competition for “growing space” algorithms (De Bruijn et al. 2014). PnET (Photosynthesis and EvapoTranspiration) is a simple, lumped parameter model of carbon and water balances of forests (Aber and Federer 1992), built on two principal relationships: 1) maximum photo-synthetic rate is a function of foliar nitrogen concentration, and 2) stomatal conductance is a function of realized photosynthetic rate. PnET-Succession uses one PnET simulation for each tree species-age cohort and uses water and light consumption by these cohorts to implement competition. PnET-Succession calculates resources in terms of Non-Structural Carbon (NSC) for each cohort in its PnET part and assumes that cohorts die when their NSC level drops below a threshold. The PnET-Succession extension also tracks dead biomass over time, divided into two pools: woody and leaf litter.

# Release Notes

- Latest official release: Version 4.0.1 — January 2021
- [View User Guide](https://github.com/LANDIS-II-Foundation/Extension-PnET-Succession/blob/master/deploy/docs/LANDIS-II%20PnET-Succession%20v4.0%20User%20Guide.pdf).
- Full release details found in the User Guide and on GitHub.

# Requirements

To use PnET-Succession, you need:

- The [LANDIS-II model v7.0](http://www.landis-ii.org/install) installed on your computer.
- Example files (see below)

# Download

Version 4.0.1 can be downloaded [here](https://github.com/LANDIS-II-Foundation/Extension-PnET-Succession/blob/master/deploy/installer/LANDIS-II-V7%20PnET-Succession%204.0.1-setup.exe). To install it on your computer, launch the installer.

# Example Files

LANDIS-II requires a global parameter file for your scenario, and separate parameter files for each extension.

Example files are [here](https://github.com/LANDIS-II-Foundation/Extension-PnET-Succession/tree/master/deploy/examples/biomass-Pnet-succession-example/PnET-succession-example.zip).

# Citation

Arjan de Bruijn, Eric J. Gustafson, Brian R. Sturtevant, Jane R. Foster, Brian R. Miranda, Nathanael I. Lichti, Douglass F. Jacobs, Toward more robust projections of forest landscape dynamics under novel environmental conditions: Embedding PnET within LANDIS-II. Ecological Modelling Volume 287, 10 September 2014, Pages 44–57

# Support

If you have a question, please contact Eric Gustafson. 
You can also ask for help in the [LANDIS-II users group](http://www.landis-ii.org/users).

If you come across any issue or suspected bug, please post about it in the [issue section of the Github repository](https://github.com/LANDIS-II-Foundation/Extension-PnET-Succession/issues) (GitID required).

# Authors

Arjan de Bruijn, Eric Gustafson, Brian Miranda, USFS and Purdue University

Mail : eric.gustafson@usda.gov
