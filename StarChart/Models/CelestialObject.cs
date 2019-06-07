using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarChart.Models
{
    public class CelestialObject
    {
       
        public int Id
        {
            get;
            set;
        }
        [Required]
        public string Name
        {
            get;
            set;
        }
        public int? OrbitedObjectId
        {
            get;
            set;
        }

        // http://www.entityframeworktutorial.net/code-first/notmapped-dataannotations-attribute-in-code-first.aspx
        // won't have a corresponding db col.
        [NotMapped]
        public List<CelestialObject> Satellites
        {
            get;
            set;
        }

        public TimeSpan OrbitalPeriod
        {
            get;
            set;
        }

        public CelestialObject()
        {
        }
    }
}
