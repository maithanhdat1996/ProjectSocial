using SocialFashion.Model.Models;
using SocialFashion.Service;
using SocialFashion.Web.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace SocialFashion.Web.Api
{
    [RoutePrefix("api/event")]
    public class EventController : ApiControllerBase
    {
        IEventService _eventService;
        public EventController(ILogService logService, IEventService eventService) :
            base(logService)
        {
            this._eventService = eventService;
        }

        [Route("getall")]
        public HttpResponseMessage Get(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {

                var listEvent = _eventService.GetAll();

                HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK, listEvent);


                return response;
            });
        }
        [Route("create")]
        public HttpResponseMessage Post(HttpRequestMessage request, Event e)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                if (ModelState.IsValid)
                {
                    request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
                else
                {
                    var _event = _eventService.Add(e);
                    _eventService.SaveChanges();

                    response = request.CreateResponse(HttpStatusCode.Created, _event);

                }
                return response;
            });
        }
        [Route("update")]
        public HttpResponseMessage Put(HttpRequestMessage request, Event e)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                if (ModelState.IsValid)
                {
                    request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
                else
                {
                    _eventService.Update(e);
                    _eventService.SaveChanges();

                    response = request.CreateResponse(HttpStatusCode.OK);

                }
                return response;
            });
        }
        [Route("delete")]
        public HttpResponseMessage Delete(HttpRequestMessage request, int id)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                if (ModelState.IsValid)
                {
                    request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
                else
                {
                    _eventService.Delete(id);
                    _eventService.SaveChanges();

                    response = request.CreateResponse(HttpStatusCode.OK);

                }
                return response;
            });
        }
    }
   
}