using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActorsParser.Details
{
    internal class Actor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Column(TypeName = "jsonb")]
        public Dictionary<string, List<Role>> Roles { get; set; }

        public Actor(int id, string name, Dictionary<string, List<Role>> titles)
        {
            Id = id;
            Name = name;
            Roles = titles;
        }

        public Actor()
        {
            Id = 0;
            Name = "";
            Roles = new Dictionary<string, List<Role>>();
        }
    }
}
