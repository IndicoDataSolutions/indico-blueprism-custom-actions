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
        /// <param name="workflowId"><c><see cref="decimal">Id</see></c> of the workflow to submit data to.</param>
        /// <returns><c><see cref="DataTable"/></c> with submissions <c><see cref="decimal">ids</see></c>.</returns>
        DataTable WorkflowFileSubmission(DataTable filepaths, decimal workflowId);

        /// <summary>
        /// Method submits resources from certain uris to the Indico app.
        /// </summary>
        /// <param name="uris"></param>
        /// <param name="workflowId"><c><see cref="decimal">Id</see></c> of the workflow to submit data to.</param>
        /// <returns><c><see cref="DataTable"/></c> with submissions <c><see cref="decimal">ids</see></c>.</returns>
        DataTable WorkflowUrisSubmission(DataTable uris, decimal workflowId);
    }
}
