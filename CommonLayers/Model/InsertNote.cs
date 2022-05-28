using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLayer.Model
{
    public class InsertNoteRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string CreatedDate { get; set; }

        public string UpdatedDate { get; set; }

        [Required]
        public string Note { get; set; }

        public string ScheduleDateTime { get; set; }
    }

    public class InsertNoteResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
