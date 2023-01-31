using System.Data;

namespace Indico.BluePrism.Connector
{
    /// <summary>
    /// <c><see cref="IIndicoConnector"/></c> defines all Indico operations.
    /// </summary>
    public interface IIndicoConnector
    {
        /// <summary>
        /// Method submits files to the Indico app.
        /// </summary>
        /// <param name="filepaths"><c><see cref="DataTable"/></c> containing paths of the files to submit.</param>
        /// <param name="uris"><c><see cref="DataTable"/></c> containing uris to the files to submit.</param>
        /// <param name="workflowId"><c><see cref="decimal">Id</see></c> of the workflow to submit data to.</param>
        /// <returns><c><see cref="DataTable"/></c> with submissions <c><see cref="decimal">ids</see></c>.</returns>
        DataTable WorkflowSubmission(DataTable filepaths, DataTable uris, decimal workflowId);

        /// <summary>
        /// Method lists submissions.
        /// </summary>
        /// <param name="submissionIds"><c><see cref="DataTable"/></c> containing ids of the submissions to list.</param>
        /// <param name="workflowIds"><c><see cref="DataTable"/></c> containing ids of workflows to include submissions from.</param>
        /// <param name="inputFileName">File name used to filter result.</param>
        /// <param name="status">Submission status used to filter result.</param>
        /// <param name="retrieved">If submission retrieved, used to filter result.</param>
        /// <param name="limit">Submission count limit. Default value is 1000.</param>
        /// <returns></returns>
        DataTable ListSubmissions(DataTable submissionIds, DataTable workflowIds, string inputFileName, string status, string retrieved, decimal limit = 1000);

        DataTable GetSubmissionFileDetails(DataTable submissionIds);
    }
}
