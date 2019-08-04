using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TreeInTheClouds_Server.Models;

namespace TreeInTheClouds_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NodesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public string DbDir { get; set; }

        private string GetDbPath(string fileName)
        {

            return DbDir + fileName;
        }

        public NodesController(IConfiguration configuration)
        {
            _configuration = configuration;
            DbDir = _configuration.GetValue<string>("DbDirectory");
        }
        // GET: api/Nodes/findAll?fileName=testdb
        [HttpGet("findAll")]
        public IEnumerable<Node> Get(string fileName)
        {
            using (var Context = new LiteDatabase(GetDbPath(@fileName)))
            {
                var Nodes = Context.GetCollection<Node>(DbHelper.NodesCollection);
                return Nodes.FindAll();
            }

            //new string[] { "value1", "value2" };
        }

        [HttpGet("findAllAsync")]
        public async Task<ActionResult<IEnumerable<Node>>> GetAsync(string fileName)
        {

            return Ok(await GetAllNodesAsync(fileName).ConfigureAwait(false));


        }

        private Task<IEnumerable<Node>> GetAllNodesAsync(string fileName)
        {
            using (var Context = new LiteDatabase(GetDbPath(@fileName)))
            {
                return Task.Run(() => {

                    var Nodes = Context.GetCollection<Node>(DbHelper.NodesCollection);
                    return Nodes.FindAll();
                });
            };


        }

        // GET: api/Nodes/find?fileName=testdb&name=TestMan
        [HttpGet("find")]
        public Node Get(string fileName, string name)
        {
            using (var Context = new LiteDatabase(GetDbPath(@fileName)))
            {

                return Context.GetCollection<Node>(DbHelper.NodesCollection).Find(x => x.Name == name).FirstOrDefault();
            }


        }


        [HttpGet("findAsync")]
        public async Task<ActionResult<Node>> GetAsync(string fileName, string name)
        {

            return Ok(await GetAsync(fileName, name).ConfigureAwait(false));

        }

        private Task<Node> GetNodeAsync(string fileName, string name)
        {
            using (var Context = new LiteDatabase(GetDbPath(@fileName)))
            {
                return Task.Run(() => {

                    var Nodes = Context.GetCollection<Node>(DbHelper.NodesCollection);
                    return Context.GetCollection<Node>(DbHelper.NodesCollection).Find(x => x.Name == name).FirstOrDefault();
                });
            };


        }


        // POST: api/Nodes/create?fileName=testdb
        [HttpPost("create")]
        public void Post(string fileName, Node Node)
        {

            using (var Context = new LiteDatabase(GetDbPath(@fileName)))
            {
                var Nodes = Context.GetCollection<Node>(DbHelper.NodesCollection);
                Nodes.Insert(Node);
                Nodes.EnsureIndex(x => x.Name);


            }
        }


        [HttpPost("createAsync")]
        public async void PostAsync(string fileName, Node Node)
        {

            await CreateNodeAsync(fileName, Node).ConfigureAwait(false);

        }


        private Task CreateNodeAsync(string fileName, Node Node)
        {
            using (var Context = new LiteDatabase(GetDbPath(@fileName)))
            {
                return Task.Run(() => {

                    var Nodes = Context.GetCollection<Node>(DbHelper.NodesCollection);
                    Nodes.Insert(Node);
                    Nodes.EnsureIndex(x => x.Name);
                });
            };

        }


        // PUT: api/Nodes/edit?fileName=testdb
        [HttpPut("edit")]
        public void Put(string fileName, Node Node)
        {
            using (var Context = new LiteDatabase(GetDbPath(@fileName)))
            {

                var Nodes = Context.GetCollection<Node>(DbHelper.NodesCollection);
                Nodes.Update(Node);
            }

        }



        [HttpPut("editAsync")]
        public async void PutAsync(string fileName, Node Node)
        {

            await EditNodeAsync(fileName, Node).ConfigureAwait(false);

        }

        private Task EditNodeAsync(string fileName, Node Node)
        {
            using (var Context = new LiteDatabase(GetDbPath(@fileName)))
            {
                return Task.Run(() => {

                    var Nodes = Context.GetCollection<Node>(DbHelper.NodesCollection);
                    Nodes.Update(Node);
                });
            };

        }



        // DELETE: api/ApiWithActions/delete?fileName=testdb&id=5
        [HttpDelete("delete")]
        public void Delete(string fileName, int id)
        {

            using (var Context = new LiteDatabase(GetDbPath(@fileName)))
            {
                var Nodes = Context.GetCollection<Node>(DbHelper.NodesCollection);
                Nodes.Delete(id);

            }
        }

        [HttpDelete("deleteAsync")]
        public async void DeleteAsync(string fileName, int id)
        {

            await DeleteNodeAsync(fileName, id).ConfigureAwait(false);

        }
        private Task DeleteNodeAsync(string fileName, int id)
        {
            using (var Context = new LiteDatabase(GetDbPath(@fileName)))
            {
                return Task.Run(() => {

                    var Nodes = Context.GetCollection<Node>(DbHelper.NodesCollection);
                    Nodes.Delete(id);
                });
            };

        }
    }
}

