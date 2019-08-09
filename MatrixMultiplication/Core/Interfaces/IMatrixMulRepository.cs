using System.Runtime.InteropServices;
using MatrixMul.Core.Model;

namespace MatrixMul.Core.Interfaces
{
    /// <summary>
    ///     This interface represents a "datastore" used to buffer the data of the computation
    ///     The ids given to the methods are guaranteed to identify this calculation exactly
    /// </summary>
    public interface IMatrixMulRepository
    {
        /// <summary>
        ///     Store a Calculation, this includes serializing it
        /// </summary>
        /// <param name="id"></param>
        /// <param name="calculation"></param>
        void StoreCalculation(string id, MatrixCalculation calculation);

        /// <summary>
        ///     Retrieve and deserialize a calculation
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        MatrixCalculation GetCalculation(string id);

        /// <summary>
        ///     Delete all data associated with
        /// </summary>
        /// <param name="id"></param>
        void DeleteCalculation(string id);

        void StoreResultMatrix(string id, Matrix matrix);
        Matrix GetResultMatrix(string id);
        bool HasResultMatrix(string id);
        void DeleteResultMatrix(string id);

        void StoreComputationTasksForWorker(string id, int workerId, ComputationTask[] tasks);
        ComputationTask[] GetComputationTasksForWorker(string id, int workerId);
        void DeleteComputationTasks(string id, int workerId);

        void StoreComputationResults(string id, int worker, ComputationResult[] results);
        ComputationResult[] GetComputationResults(string id, int worker);
        void DeleteComputationResults(string id, int workerid);
    }
}