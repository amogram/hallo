using System.Collections.Generic;
using System.Linq;

namespace Hallo.Test.Integration.TestApi
{
    public class PersonRepresentation : Hal<Person>
    {
        private readonly ContactLookup _contacts;

        public PersonRepresentation(ContactLookup contacts)
        {
            _contacts = contacts;
        }
    
        protected override IEnumerable<Link> LinksFor(Person resource)
        {
            yield return new Link("self", $"/people/{resource.Id}");
        }

        protected override object EmbeddedFor(Person resource)
        {
            return new
            {
                contacts = _contacts.For(resource)
            };
        }
    }

    public abstract class PagedListRepresentation<TItem> : Hal<PagedList<TItem>>
    {
        private readonly string _baseUrl;
        private readonly IHal _itemRepresentation;

        protected PagedListRepresentation(string baseUrl, IHal itemRepresentation)
        {
            _baseUrl = baseUrl;
            _itemRepresentation = itemRepresentation;
        }
        
        protected override object StateFor(PagedList<TItem> resource)
        {
            return new
            {
                resource.CurrentPage,
                resource.TotalItems,
                resource.TotalPages
            };
        }

        protected override object EmbeddedFor(PagedList<TItem> resource)
        {
            var items = from item in resource.Items
                        let state = _itemRepresentation.StateFor(item)
                        let links = _itemRepresentation.LinksFor(item)
                        select new HalRepresentation(state, links);

            return new
            {
                Items = items.ToList()
            };
        }

        protected override IEnumerable<Link> LinksFor(PagedList<TItem> resource)
        {
            yield return new Link("self", $"{_baseUrl}?page={resource.CurrentPage}");
            yield return new Link("first", $"{_baseUrl}?page=1");
            yield return new Link("last", $"{_baseUrl}?page={resource.TotalPages}");

            if (resource.CurrentPage > 1)
            {
                yield return new Link("prev", $"{_baseUrl}?page={resource.CurrentPage - 1}");
            }

            if (resource.CurrentPage < resource.TotalPages)
            {
                yield return new Link("next", $"{_baseUrl}?page={resource.CurrentPage + 1}");
            }
        }
    }

    public class PersonListRepresentation : PagedListRepresentation<Person>
    {
        public PersonListRepresentation(Hal<Person> personRepresentation) 
            : base("/people", personRepresentation) { }
    }
}