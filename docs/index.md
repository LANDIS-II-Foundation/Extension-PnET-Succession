# What is the PnET-Succession Extension?

The  PnET-Succession extension implements succession with cohorts defined by age ranges and including biomass per cohort. Most parameters are able to change over time, due to climate change for example.

The PnET-Succession extension was originally based on the Biomass Succession extension of Scheller and Mladenoff (2004), embedding elements of the PnET-II ecophysiology model of Aber et al (1995) to simulate growth as a competition for available light and water, replacing the existing competition for “growing space” algorithms (De Bruijn et al. 2014). PnET (Photosynthesis and EvapoTranspiration) is a simple, lumped parameter model of carbon and water balances of forests (Aber and Federer 1992), built on two principal relationships: 1) maximum photo-synthetic rate is a function of foliar nitrogen concentration, and 2) stomatal conductance is a function of realized photosynthetic rate. PnET-Succession uses one PnET simulation for each tree species-age cohort and uses water and light consumption by these cohorts to implement competition. PnET-Succession calculates resources in terms of Non-Structural Carbon (NSC) for each cohort in its PnET part and assumes that cohorts die when their NSC level drops below a threshold. The PnET-Succession extension also tracks dead biomass over time, divided into two pools: woody and leaf litter.

# Release Notes

- Latest official release: Version 5.1 — September 2023
- Download the User Guide here: [PnET-Succession v5.1 User Guide](https://github.com/LANDIS-II-Foundation/Extension-PnET-Succession/blob/master/deploy/docs/LANDIS-II%20PnET-Succession%20v5.1%20User%20Guide.pdf).
- Full release details can be found in the User Guide and on [GitHub](https://github.com/LANDIS-II-Foundation/Extension-PnET-Succession).
- A detailed description of the model can be found in this updated document: [Description of PnET-Succession v5.1.pdf](https://github.com/LANDIS-II-Foundation/Foundation-Publications/blob/main/Description%20of%20PnET-Succession%20v5.1.pdf)
- This version uses custom code libraries that make it incompatible with some other "Official Release" versions of extensions available on the LANDIS-II site.  There are not compatibility issues for base (age-only) extensions, but only for "biomass-compatible" extensions.  To allow users to use these biomass extensions with this version of PnET-Succession, we have compiled compatible versions of multiple extensions.  These extension versions are not "official releases" and are meant to temporarily fulfill the needs of users until the next official release occurs.  The compatible extension installers can be found on GitHub in this folder: [v5.1_installers](https://github.com/LANDIS-II-Foundation/Extension-PnET-Succession/tree/master/deploy/v5.1_installers).  Please read the ReadMe.txt file for important installation and usage information.

# Requirements

To use PnET-Succession, you need:

- The [LANDIS-II model v7.0](http://www.landis-ii.org/install) installed on your computer.


# Download

Version 5.1 installer can be downloaded here:  [PnET-Succession v5.1 Installer](https://github.com/LANDIS-II-Foundation/Extension-PnET-Succession/blob/master/deploy/installer/LANDIS-II-V7%20PnET-Succession%205.1-setup.exe). To install it on your computer, launch the installer.

# Example Files

LANDIS-II requires a global parameter file for your scenario, and separate parameter files for each extension.

Example files using PnET-Succession are here: [Examples](https://github.com/LANDIS-II-Foundation/Extension-PnET-Succession/tree/master/deploy/examples/biomass-Pnet-succession-example/PnET-succession-example.zip).

# Citation

Gustafson, Eric J., Brian R. Sturtevant, Brian R. Miranda, Zaixing Zhou.  2023.  PnET-Succession v 5.1:  Comprehensive description of an ecophysiological succession extension within the LANDIS-II forest landscape model.  Published by the LANDIS-II Foundation.  URL: [https://github.com/LANDIS-II-Foundation/Foundation-Publications/blob/main/Description of PnET-Succession v5.1.pdf](https://github.com/LANDIS-II-Foundation/Foundation-Publications/blob/main/Description%20of%20PnET-Succession%20v5.1.pdf).

# Support

If you have a question, please contact Eric Gustafson. 
You can also ask for help in the [LANDIS-II users group](http://www.landis-ii.org/users).

If you come across any issue or suspected bug, please post about it in the [issue section of the Github repository](https://github.com/LANDIS-II-Foundation/Extension-PnET-Succession/issues) (GitID required).

# Authors

Arjan de Bruijn, Eric Gustafson, Brian Miranda, Brian Sturtevant; USFS and Purdue University

Mail : eric.gustafson@usda.gov
