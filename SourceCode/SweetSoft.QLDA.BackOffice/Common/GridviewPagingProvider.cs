using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SweetSoft.QLDA.BackOffice.Common
{
    /// <summary>
    /// Class generating a enumerable list of pages
    /// </summary>
    /// <remarks>
    /// Primarily used by the thumbnail list to create a paging control
    /// </remarks>
    public class PagingProvider
    {
        /// <summary>
        /// Creates a list of page numbers to be enumerated in a paging control.
        /// </summary>
        /// <remarks>
        /// Paging is 1-based, meaning that the first page is called page 1.
        /// </remarks>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="totalItems">The total items.</param>
        /// <param name="currentPage">The current page.</param>
        /// <returns></returns>
        public static List<PageItem> CreatePages(int pageSize, int totalItems, int currentPage)
        {
            List<PageItem> pages = new List<PageItem>();
            int totalPages = (totalItems / pageSize);
            if (totalItems % pageSize > 0)
                totalPages += 1;

            int startIndex = 0;
            int endIndex = totalPages;

            if (totalPages > 5)
            {
                startIndex = currentPage - 3;
                endIndex = currentPage + 2;
                if (startIndex < 0)
                {
                    startIndex = 0;
                    endIndex = startIndex + 5;
                }
                if (endIndex > totalPages)
                {
                    endIndex = totalPages;
                    startIndex = totalPages - 5;
                }
            }

            pages.Add(new PageItem { Text = "««", IsFirst = true, Title = "Go to first page", PageNum = "1", CurrentPage = currentPage == 1 });
            pages.Add(new PageItem { Text = "«", Title = "Go to previous page", PageNum = (currentPage - 1).ToString(), CurrentPage = currentPage == 1 });

            for (int i = startIndex; i < endIndex; i++)
            {
                PageItem page = new PageItem { Text = (i + 1).ToString(), PageNum = (i + 1).ToString(), CurrentPage = i == (currentPage - 1) };
                pages.Add(page);
            }

            pages.Add(new PageItem { Text = "»", Title = "Go to next page", PageNum = (currentPage + 1).ToString(), CurrentPage = currentPage == totalPages });
            pages.Add(new PageItem { Text = "»»", IsLast = true, Title = "Go to last page", PageNum = totalPages.ToString(), CurrentPage = currentPage == totalPages });
            return pages;
        }
    }

    /// <summary>
    /// Page class containing the information used to create a paging control
    /// </summary>
    public class PageItem
    {
        /// <summary>
        /// Gets or sets the text content.
        /// </summary>
        /// <value>The text content.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the page number.
        /// </summary>
        /// <value>The page num.</value>
        public string PageNum { get; set; }

        public bool IsFirst { get; set; }
        public bool IsLast { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this page is the current page.
        /// </summary>
        /// <value><c>true</c> if [current page]; otherwise, <c>false</c>.</value>
        public bool CurrentPage { get; set; }
    }
}