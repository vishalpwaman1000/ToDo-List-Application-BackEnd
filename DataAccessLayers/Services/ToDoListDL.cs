using CommonLayer.Model;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class ToDoListDL : IToDoListDL
    {
        private readonly IConfiguration _configuration;
        private readonly MongoClient _mongoClient;
        private readonly IMongoCollection<InsertNoteRequest> _mongoCollection;
        public ToDoListDL(IConfiguration configuration)
        {
            _configuration = configuration;
            _mongoClient = new MongoClient(_configuration["DatabaseSettings:ConnectionString"]);
            var _MongoDatabase = _mongoClient.GetDatabase(_configuration["DatabaseSettings:DatabaseName"]);
            _mongoCollection = _MongoDatabase.GetCollection<InsertNoteRequest>(_configuration["DatabaseSettings:CollectionName"]);
        }

        public async Task<InsertNoteResponse> InsertNote(InsertNoteRequest request)
        {
            InsertNoteResponse response = new InsertNoteResponse();
            response.IsSuccess = true;
            response.Message = "Insert Note Successfully.";

            try
            {
                request.CreatedDate = DateTime.Now.ToString("dddd, dd-MM-yyyy hh:mm tt");
                request.UpdatedDate = null;
                await _mongoCollection.InsertOneAsync(request);

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Exception Occurs : " + ex.Message;
            }

            return response;
        }

        public async Task<GetNoteResponse> GetNote(GetNoteRequest request)
        {
            GetNoteResponse response = new GetNoteResponse();
            response.IsSuccess = true;
            response.Message = "Fetch Data Successfully.";

            try
            {
                long Count = await _mongoCollection.CountAsync(x => true);
                if (Count == 0)
                {
                    response.Message = "No Data Found";
                    return response;
                }
                request.PageNumber = request.PageNumber == 0 ? 1 : request.PageNumber;
                string SortCondition = request.SortBy.ToLower() == "asc" ? "{_id:1}" : "{_id:-1}";

                int Offset = (request.PageNumber - 1) * request.NumberOfRecordPerPage;

                List<InsertNoteRequest> DataList = new List<InsertNoteRequest>();
                DataList = await _mongoCollection.Find(x => true)
                                                      .Skip(Offset)
                                                      .Limit(request.NumberOfRecordPerPage)
                                                      .Sort(SortCondition)
                                                      .ToListAsync();

                response.CurrentPage = request.PageNumber;
                response.TotalRecords = (decimal)Count;
                response.TotalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(response.TotalRecords / request.NumberOfRecordPerPage)));

                if (DataList.Count == 0)
                {
                    return response;
                }
                response.data = new List<GetNote>();
                int NoteId = request.SortBy.ToLower() == "asc" ? (request.PageNumber - 1) * request.NumberOfRecordPerPage + 1 : (int)Count - ((request.PageNumber - 1) * request.NumberOfRecordPerPage);
                foreach (InsertNoteRequest data in DataList)
                {
                    string[] DateTime = data.ScheduleDateTime.Split(" ");
                    response.data.Add(
                        new GetNote()
                        {
                            Id = data.Id,
                            NoteId = request.SortBy.ToLower() == "asc" ? NoteId++ : NoteId--,
                            Note = data.Note,
                            ScheduleDate = DateTime[2] + "-" + DateTime[1] + "-" + DateTime[3], //Sat May 28 2022 15:02:11
                            ScheduleTime = DateTime[4],
                        }); ;

                }

                


            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Exception Occurs : " + ex.Message;
            }

            return response;
        }

        public async Task<GetNoteByIdResponse> GetNoteById(string Id)
        {
            GetNoteByIdResponse response = new GetNoteByIdResponse();
            response.IsSuccess = true;
            response.Message = "Get Note By Id Successfully";

            try
            {
                response.data = new InsertNoteRequest();
                response.data = await _mongoCollection.Find(x => x.Id == Id).FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Exception Occurs : " + ex.Message;
            }

            return response;
        }

        public async Task<UpdateNoteResponse> UpdateNote(InsertNoteRequest request)
        {
            UpdateNoteResponse response = new UpdateNoteResponse();
            response.IsSuccess = true;
            response.Message = "Update Note Successfully.";
            try
            {

                GetNoteByIdResponse GetRecord = await GetNoteById(request.Id);
                request.CreatedDate = GetRecord.data.CreatedDate;
                request.UpdatedDate = DateTime.Now.ToString("dddd, dd-MMM-yyyy hh:mm tt");
                var Result = await _mongoCollection.ReplaceOneAsync(x => x.Id == request.Id, request);
                if (!Result.IsAcknowledged)
                {
                    response.Message = "Something Went Wrong";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Exception Occurs : " + ex.Message;
            }

            return response;
        }

        public async Task<DeleteNoteResponse> DeleteNote(string Id)
        {
            DeleteNoteResponse response = new DeleteNoteResponse();
            response.IsSuccess = true;
            response.Message = "Delete Note Successfully";

            try
            {

                var Result = await _mongoCollection.DeleteOneAsync(x => x.Id == Id);
                if (!Result.IsAcknowledged)
                {
                    response.Message = "Invalid Id Please Enter Valid Id";
                }

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Exception Occurs : " + ex.Message;
            }

            return response;
        }
    }
}
