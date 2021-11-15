using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.Diagnostics;

namespace Madingley
{
    /// <summary>
    /// A revised version of the herbivory process, written November 2011
    /// </summary>
    public partial class RevisedHerbivory: IEatingImplementation
    {
        /// <summary>
        /// The time unit associated with this herbivory implementation and its parameters
        /// </summary>
        private string _TimeUnitImplementation;
        /// <summary>
        /// Get the time unit associated with this herbivory implementation and its parameters
        /// </summary>
        public string TimeUnitImplementation { get { return _TimeUnitImplementation; } }

        /// <summary>
        /// The assimilation efficiency of eaten autotroph mass into herbivore body mass
        /// </summary>
        private double _AssimilationEfficiency;
        /// <summary>
        /// Get and set the assimilation efficiency of eaten autotroph mass into herbivore body mass
        /// </summary>
        public double AssimilationEfficiency
        {
            get { return _AssimilationEfficiency; }
            set { _AssimilationEfficiency = value; }
        }

        /// <summary>
        /// The scalar of the relationship between handling time and the function of herbivore mass for the terrestrial realm
        /// </summary>
        private double _HandlingTimeScalarTerrestrial;
        /// <summary>
        /// Get the scalar of the relationship between handling time and the function of herbivore mass for the terrestrial realm
        /// </summary>
        public double HandlingTimeScalarTerrestrial
        { get { return _HandlingTimeScalarTerrestrial; } }

        /// <summary>
        /// The scalar of the relationship between handling time and the function of herbivore mass for the marine realm
        /// </summary>
        private double _HandlingTimeScalarMarine;
        /// <summary>
        /// Get the scalar of the relationship between handling time and the function of herbivore mass for the marine realm
        /// </summary>
        public  double HandlingTimeScalarMarine
        { get { return _HandlingTimeScalarMarine; } }
        

        /// <summary>
        /// The exponent applied to herbivore mass in the handling time relationship for the terrestrial realm
        /// </summary>
        private double _HandlingTimeExponentTerrestrial;
        /// <summary>
        /// Get the exponent applied to herbivore mass in the handling time relationship for the terrestrial realm
        /// </summary>
        public double HandlingTimeExponentTerrestrial
        { get { return _HandlingTimeExponentTerrestrial; } }

        /// <summary>
        /// The exponent applied to herbivore mass in the handling time relationship for the marine realm
        /// </summary>
        private double _HandlingTimeExponentMarine;
        /// <summary>
        /// The exponent applied to herbivore mass in the handling time relationship for the marine realm
        /// </summary>
        public double HandlingTimeExponentMarine
        { get { return _HandlingTimeExponentMarine; } }
        

        /// <summary>
        /// Reference mass of plant matter for calculating handling times
        /// </summary>
        public double _ReferenceMass;
        /// <summary>
        /// Get the reference mass of plant matter for calculating handling times
        /// </summary>
        public double ReferenceMass {  get { return _ReferenceMass; } }
        
        /// <summary>
        /// The maximum herbivory rate for a herbivore of 1 g
        /// </summary>
        private double _HerbivoryRateConstant;
        /// <summary>
        /// Get the maximum herbivory rate for a herbivore of 1 g
        /// </summary>
        public double HerbivoryRateConstant
        { get { return _HerbivoryRateConstant; } }

        /// <summary>
        /// The exponent to apply to body mass in the relationship between body mass and herbivory rate
        /// </summary>
        private double _HerbivoryRateMassExponent;
        /// <summary>
        /// Get and set the exponent to apply to body mass in the relationship between body mass and herbivory rate
        /// </summary>
        public double HerbivoryRateMassExponent
        {   get { return _HerbivoryRateMassExponent; }      }

        /// <summary>
        /// The exponent applied to prey density when calculating attack rates for organisms in the terrestrial realm
        /// </summary>
        private double _AttackRateExponentTerrestrial;
        /// <summary>
        /// Get and set the exponent applied to prey density when calculating attack rates for organisms in the terrestrial realm
        /// </summary>
        public double AttackRateExponentTerrestrial
        {    get { return _AttackRateExponentTerrestrial; }  }

        /// <summary>
        /// The exponent applied to prey density when calculating attack rates for organisms in the marine realm
        /// </summary>
        private double _AttackRateExponentMarine;

        /// <summary>
        /// Get and set the exponent applied to prey density when calculating attack rates for organisms in the marine realm
        /// </summary>
        public double AttackRateExponentMarine
        {  get { return _AttackRateExponentMarine; }  }

        // Variable to hold the instantaneous fraction of the autotroph stock biomass that is eaten
        double InstantFractionEaten;

        /// <summary>
        /// Boltzmann's constant
        /// </summary>
        private double _BoltzmannConstant;

        /// <summary>
        /// Boltzmann's constant
        /// </summary>
        public double BoltzmannConstant
        { get { return _BoltzmannConstant; } }

        /// <summary>
        /// The activation energy of handling time
        /// </summary>
        private double _ActivationEnergyHandlingTime;

        /// <summary>
        /// Get the activation energy of handling time
        /// </summary>
        public double ActivationEnergyHandlingTime
        { get { return _ActivationEnergyHandlingTime; } }

        /// <summary>
        /// The activation energy for herbivory rate
        /// </summary>
        private double _ActivationEnergyHerbivoryRate;

        /// <summary>
        /// Get the activation energy for herbivory rate
        /// </summary>
        public double ActivationEnergyHerbivoryRate
        {
            get { return _ActivationEnergyHerbivoryRate; }
            set { _ActivationEnergyHerbivoryRate = value; }
        }

        /// <summary>
        /// Reference temperature for calculating Arrhenius term
        /// </summary>
        public double _ReferenceTemperature;
        /// <summary>
        /// Get the reference mass of plant matter for calculating handling times
        /// </summary>
        public double ReferenceTemperature { get { return _ReferenceTemperature; } }

        public void InitialiseParametersHerbivory()
        {
            _TimeUnitImplementation =
                EcologicalParameters.TimeUnits[(int)EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.TimeUnitImplementation"]];
            _HandlingTimeScalarTerrestrial = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.Terrestrial.HandlingTimeScalar"];
            _HandlingTimeScalarMarine = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.Marine.HandlingTimeScalar"];
            _HandlingTimeExponentTerrestrial = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.Terrestrial.HandlingTimeExponent"];
            _HandlingTimeExponentMarine = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.Marine.HandlingTimeExponent"];
            _HerbivoryRateConstant = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.RateConstant"];
            _HerbivoryRateMassExponent = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.RateMassExponent"];
            _AttackRateExponentTerrestrial = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.Terrestrial.AttackRateExponent"];
            _AttackRateExponentMarine = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.Marine.AttackRateExponent"];
            _ReferenceMass = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.HandlingTimeReferenceMass"];
            _BoltzmannConstant = EcologicalParameters.Parameters["BoltzmannConstant"];
            _ActivationEnergyHandlingTime = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.ActivationEnergyHandlingTime"];
            _ActivationEnergyHerbivoryRate = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.ActivationEnergyHerbivoryRate"];
            _ReferenceTemperature = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.ReferenceTemperature"];
        }


        /// <summary>
        /// Write out the values of the parameters to an output file
        /// </summary>
        /// <param name="sw">A streamwriter object to write the parameter values to</param>
        public void WriteOutParameterValues(StreamWriter sw)
        {
            // Write out parameters
            sw.WriteLine("Herbivory\tTimeUnitImplementation\t" + Convert.ToString(_TimeUnitImplementation));
            sw.WriteLine("Herbivory\tReferenceMass_g\t" + Convert.ToString(_ReferenceMass));
            sw.WriteLine("Herbivory\tHandlingTimeScalarTerrestrial\t" + Convert.ToString(_HandlingTimeScalarTerrestrial));
            sw.WriteLine("Herbivory\tHandlingTimeScalarMarine\t" + Convert.ToString(_HandlingTimeScalarMarine));
            sw.WriteLine("Herbivory\tHandlingTimeExponentTerrestrial\t" + Convert.ToString(_HandlingTimeExponentTerrestrial));
            sw.WriteLine("Herbivory\tHandlingTimeExponentMarine\t" + Convert.ToString(_HandlingTimeExponentMarine));
            sw.WriteLine("Herbivory\tHerbivoryRateConstant\t" + Convert.ToString(_HerbivoryRateConstant));

            sw.WriteLine("Herbivory\t_AttackRateExponentTerrestrial\t" + Convert.ToString(HerbivoryRateMassExponent));
            sw.WriteLine("Herbivory\t_AttackRateExponentMarine\t" + Convert.ToString(HerbivoryRateMassExponent));
            sw.WriteLine("Herbivory\tHerbivoryRateMassExponent\t" + Convert.ToString(HerbivoryRateMassExponent));
            sw.WriteLine("Herbivory\tHerbivoryActivationEnergyHerbivoryRate\t" + Convert.ToString(_ActivationEnergyHerbivoryRate));
        }



        /// <summary>
        /// Calculates the potential biomass of an autotroph stock eaten by a herbivore cohort (terrestrial)
        /// </summary>
        /// <param name="autotrophBiomass">The total biomass of the autotroph stock</param>
        /// <param name="herbivoreIndividualMass">The individual body mass of the acting (herbivore) cohort</param>
        /// <returns>The potential biomass eaten by the herbivore cohort</returns>
        private double CalculatePotentialBiomassEatenTerrestrial(double autotrophBiomass, double herbivoreIndividualMass,
           double temperature)
        {
            // Calculate the inidividual herbivory rate per unit autotroph mass-density per hectare
            double IndividualHerbivoryRate = CalculateIndividualHerbivoryRatePerHectare(herbivoreIndividualMass, temperature);

            // Calculate autotroph biomass density per hectare
            double AutotrophBiomassDensity = autotrophBiomass / _CellAreaHectares;
            
            // Calculate the expected autotroph biomass eaten
            return IndividualHerbivoryRate * Math.Pow(AutotrophBiomassDensity, _AttackRateExponentTerrestrial);
        }

        /// <summary>
        /// Calculates the potential biomass of an autotroph stock eaten by a herbivore cohort (marine)
        /// </summary>
        /// <param name="autotrophBiomass">The total biomass of the autotroph stock</param>
        /// <param name="herbivoreIndividualMass">The individual body mass of the acting (herbivore) cohort</param>
        /// <returns>The potential biomass eaten by the herbivore cohort</returns>
        private double CalculatePotentialBiomassEatenMarine(double autotrophBiomass, double herbivoreIndividualMass,
            double temperature)
        {
            // Calculate the inidividual herbivory rate per unit autotroph mass-density per hectare
            double IndividualHerbivoryRate = CalculateIndividualHerbivoryRatePerHectare(herbivoreIndividualMass, temperature);

            // Calculate autotroph biomass density per hectare
            double AutotrophBiomassDensity = autotrophBiomass / _CellAreaHectares;

            // Calculate the expected autotroph biomass eaten
            return IndividualHerbivoryRate * Math.Pow(AutotrophBiomassDensity, _AttackRateExponentMarine);
        }


        /// <summary>
        /// Calculate the herbivory rate of an individual herbivore per unit autotroph mass-density per hectare
        /// </summary>
        /// <param name="herbivoreIndividualMass">Herbivore individual body mass</param>
        /// <returns>The herbivory rate of an individual herbivore per unit autotroph mass-density per hectare</returns>
        private double CalculateIndividualHerbivoryRatePerHectare(double herbivoreIndividualMass, double temperature)
        {

            //////////////JUNE 2020////////////////////////
            ///attack rate for herbivores: converted to original with temperature dependence
            ///removed exponent term * Math.Pow(((herbivoreIndividualMass)), (_HerbivoryRateMassExponent)) 
            ///will use original madingley parameters for herbivore constant and new activation energy

            return _HerbivoryRateConstant * (herbivoreIndividualMass) * Math.Exp((_ActivationEnergyHerbivoryRate * (temperature - ReferenceTemperature)) / (_BoltzmannConstant * (temperature + 273) * (ReferenceTemperature + 273)));


            //master: HerbivoryRateMassExponent=1 in model setup and is ignored in teh description on herbivore model of attack rates in PLOS Bio suppl material

        }

        /// <summary>
        /// Calculate the time taken for a herbivore in the marine realm to handle unit mass (1 g) of autotroph mass
        /// </summary>
        /// <param name="herbivoreIndividualMass">The body mass of an individual herbivore</param>
        /// <returns>The time taken for a herbivore to handle unit mass (1 g) of autotroph mass</returns>
        private double CalculateHandlingTimeMarine(double herbivoreIndividualMass)
        {
            return _HandlingTimeScalarMarine * Math.Pow((_ReferenceMass / herbivoreIndividualMass), _HandlingTimeExponentMarine);
        }


        /// <summary>
        /// Calculate the time taken for a herbivore in the terrestrial realm to handle unit mass (1 g) of autotroph mass
        /// </summary>
        /// <param name="herbivoreIndividualMass">The body mass of an individual herbivore</param>
        /// <returns>The time taken for a herbivore to handle unit mass (1 g) of autotroph mass</returns>
        private double CalculateHandlingTimeTerrestrial(double herbivoreIndividualMass, double temperature)////change here and activation energy 
        {

            ///////JUNE 2020//////////////////////////////
            ///removed temperature dependence; this would revert it to original madinlgey formulation:
            //////will use original madingley parameters for herbivore constant and exponent. Checked that _HandlingTimeScalarTerrestrial and _HandlingTimeExponentTerrestrial are only used for herbivores
            return _HandlingTimeScalarTerrestrial * Math.Pow((_ReferenceMass / (herbivoreIndividualMass)), _HandlingTimeExponentTerrestrial);

        }

        /// <summary>
        /// Calculate the actual biomass eaten by a herbivore cohort from an autotroph stock
        /// </summary>
        /// <param name="potentialBiomassEaten">The potential biomass eaten by the herbivore cohort from the autotroph stock given the encounter rate</param>
        /// <param name="totalHandlingTime">The total time that would be taken to handle all encountered autotroph biomass in all autotroph stocks</param>
        /// <param name="herbivoreAbundance">The number of individuals in the acting herbivore cohort</param>
        /// <param name="autotrophBiomass">The total biomass in the autotroph stock</param>
        /// <returns>The biomass eaten by the herbivore cohort from the autotroph stock</returns>
        private double CalculateBiomassesEaten(double potentialBiomassEaten, double totalHandlingTime, double herbivoreAbundance, double autotrophBiomass)
        {
            // Check whether there is any biomass in the autotroph stock
            if (autotrophBiomass > 0.0)
            {
                // Calculate the instantaneous fraction of the autotroph stock eaten
                InstantFractionEaten = herbivoreAbundance * ((potentialBiomassEaten / (1 + totalHandlingTime)) / autotrophBiomass);
            }
            else
            {
                // Set the instantaneous fraction of the autotroph stock eaten to zero
                InstantFractionEaten = 0.0;
            }
            
            // Return the total  biomass of the autotroph stock eaten
            return autotrophBiomass * (1 - Math.Exp(-InstantFractionEaten * _DeltaT * _ProportionOfTimeEating));
        }

    }
}
