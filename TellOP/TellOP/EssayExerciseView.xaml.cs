// <copyright file="EssayExerciseView.xaml.cs" company="University of Murcia">
// Copyright © 2016 University of Murcia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// <author>Mattia Zago</author>

namespace TellOP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using DataModels;
    using DataModels.Activity;
    using DataModels.Enums;
    using Xamarin.Forms;
    using API;
    using DataModels.APIModels.Exercise;
    using DataModels.SQLiteModels;

    /// <summary>
    /// EssayExercise application view.
    /// </summary>
    public partial class EssayExerciseView : ContentPage
    {
        /// <summary>
        /// EssayExercise object
        /// </summary>
        private EssayExercise ex;
        private bool _isAnotherAnalysisRunning = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="EssayExerciseView"/> class.
        /// </summary>
        /// <param name="essay">EssayExercise object</param>
        public EssayExerciseView(EssayExercise essay)
        {
            if (essay == null)
            {
                throw new ArgumentNullException("ex");
            }

            this.InitializeComponent();

            this.FillData(essay);

            this.searchButton.Icon = "toolbar_search.png";
            this.analysisButton.Icon = "toolbar_analysis.png";

            this.analysisButton.Clicked += this.AnalysisButton_Clicked;
            this.searchButton.Clicked += this.SearchButton_Clicked;
            this.submitButton.Clicked += this.SubmitButton_Clicked;
            this.saveButton.Clicked += this.SaveButton_Clicked;
            this.loadButton.Clicked += this.LoadButton_Clicked;

            this._changeActivityIndicatorsStatus(false);
        }

        /// <summary>
        /// Gets the title of the exercise.
        /// </summary>
        public string ExTitle
        {
            get { return this.ex.Title; }
        }

        /// <summary>
        /// Gets the language of the exercise.
        /// </summary>
        public SupportedLanguage ExLanguage
        {
            get { return this.ex.Language; }
        }

        /// <summary>
        /// Gets the level of the exercise.
        /// </summary>
        public LanguageLevelClassification ExLevel
        {
            get { return this.ex.Level; }
        }

        /// <summary>
        /// Gets the type of the exercise.
        /// </summary>
        public string ExTypeOfExercise
        {
            get { return this.ex.ToNiceString(); }
        }

        /// <summary>
        /// Gets a description of the exercise.
        /// </summary>
        public string ExDescription
        {
            get { return this.ex.Description; }
        }

        /// <summary>
        /// Gets the contents of the exercise.
        /// </summary>
        public string ExContent
        {
            get
            {
                if (this.ex is EssayExercise)
                {
                    EssayExercise eex = (EssayExercise)this.ex;
                    if (string.IsNullOrEmpty(eex.Contents))
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return eex.Contents;
                    }
                }
                else
                {
                    throw new NotSupportedException(
                        "EssayExerciseView supports only string content at "
                        + "this time");
                }
            }
        }

        /// <summary>
        /// Gets the exercise identifier.
        /// </summary>
        public int ExUID
        {
            get
            {
                return this.ex.Uid;
            }
        }

        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender">Sender param</param>
        /// <param name="e">evet param</param>
        private async void LoadButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                ExerciseAPI exAPI = new ExerciseAPI(App.OAuth2Account, this.ex.Uid);
                Exercise result = await exAPI.CallEndpointAsExerciseModel();
                if (!(result is EssayExercise))
                {
                    throw new NotImplementedException("The returned exercise type is not supported yet");
                }

                await this.FillData((EssayExercise)result);
            }
            catch (NotImplementedException ex)
            {
                await Tools.Logger.LogWithErrorMessage(this, "LoadButton_Clicked - This shouln't happen!", ex);
            }
            catch (UnsuccessfulAPICallException ex)
            {
                Tools.Logger.Log(this, "LoadButton_Clicked method", ex);
            }
            catch (Exception ex)
            {
                Tools.Logger.Log(this, "LoadButton_Clicked method", ex);
            }
        }

        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender">Sender param</param>
        /// <param name="e">evet param</param>
        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            bool userChoice = await this.DisplayAlert("Save", "Do you want to save and override the previous content?", "Save and override", "Cancel");
            if (userChoice)
            {
                try
                {
                    // Save the current status
                    await this.AnalyzeText();

                    ExerciseSubmissionAPI exSubAPI = new ExerciseSubmissionAPI(
                        App.OAuth2Account,
                        new UserActivityEssay() { ActivityID = this.ex.Uid, Text = this.ex.Contents });

                    await exSubAPI.CallEndpointAsync();
                }
                catch (OperationCanceledException ex)
                {
                    await Tools.Logger.LogWithErrorMessage(this, "Analysis Aborted", ex);
                }
                catch (UnsuccessfulAPICallException ex)
                {
                    await Tools.Logger.LogWithErrorMessage(this, "Error while saving the exercise in the database.", ex);
                }
                catch (Exception ex)
                {
                    await Tools.Logger.LogWithErrorMessage(this, "Unknown exception.", ex);
                }
            }
        }

        /// <summary>
        /// Event handler for the changed text
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">TextChanged event arg</param>
        private void ExContentEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            int count = Regex.Matches(this.ExContentEditor.Text, "\\w+").Cast<Match>().Select(match => match.Value).Count();
            double value = (count - this.ex.MinimumWords) / (double)this.ex.MaximumWords;

            if (value <= 0 || value >= 1)
            {
                this.ex.Status = ExerciseStatus.NotCompleted;
                this.ProgressBar.BackgroundColor = ExerciseStatus.NotCompleted.ToColor();
            }
            else
            {
                this.ex.Status = ExerciseStatus.Satisfactory;
                this.ProgressBar.BackgroundColor = ExerciseStatus.Satisfactory.ToColor();
            }

            if (value < 0)
            {
                value = 0d;
            }

            if (value > 1)
            {
                value = 1;
            }

            this.ProgressBar.Progress = value;

            this.ex.Contents = this.ExContentEditor.Text;
        }

        /// <summary>
        /// Fill the fields with the appropriate data
        /// </summary>
        /// <param name="essex"><see cref="EssayExercise"/> param</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task FillData(EssayExercise essex)
        {
            this.ex = essex;

            this.Title = this.ExTypeOfExercise + " " + this.ExLevel;
            this.ExTitleLabel.Text = this.ExTitle;
            this.ExDescriptionLabel.Text = string.Format(Properties.Resources.EssayExercise_MinMaxWords, this.ex.MinimumWords, this.ex.MaximumWords);
            this.ExContentEditor.Text = this.ExContent;

            this.ProgressBar.BackgroundColor = this.ex.StatusColor;

            this.ExContentEditor.TextChanged += this.ExContentEditor_TextChanged;

            // Reanalize only if the exercise was already (partially) done
            if (this.ex.Contents.Length > 0)
            {
                try
                {
                    await this.AnalyzeText();
                }
                catch (OperationCanceledException ex)
                {
                    Tools.Logger.Log(this, ex);
                }
            }
        }

        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs param</param>
        private async void SearchButton_Clicked(object sender, EventArgs e)
        {
            await this.Navigation.PushModalAsync(new MainSearch());
        }

        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs param</param>
        private async void AnalysisButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await this.AnalyzeText();
            }
            catch (OperationCanceledException ex)
            {
                await Tools.Logger.LogWithErrorMessage(this, "Operation Aborted", ex);
            }
        }

        private async void SubmitButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await this.AnalyzeText();
            }
            catch (OperationCanceledException ex)
            {
                await Tools.Logger.LogWithErrorMessage(this, "Operation Aborted", ex);
            }

            this.Navigation.PushAsync(new ExerciseAnalysis(this.ex));
        }

        /// <summary>
        /// Enable or disable the activity indicator icon.
        /// </summary>
        /// <param name="activate">True = show activity indicator.</param>
        private void _changeActivityIndicatorsStatus(bool activate)
        {
            this.aiNouns.IsVisible = activate;
            this.aiNouns.IsRunning = activate;
            this.statNouns.IsVisible = !activate;

            this.aiAdverbs.IsVisible = activate;
            this.aiAdverbs.IsRunning = activate;
            this.statAdverbs.IsVisible = !activate;

            this.aiAdjective.IsVisible = activate;
            this.aiAdjective.IsRunning = activate;
            this.statAdjective.IsVisible = !activate;

            this.aiVerbs.IsVisible = activate;
            this.aiVerbs.IsRunning = activate;
            this.statVerbs.IsVisible = !activate;

            this.aiUnknown.IsVisible = activate;
            this.aiUnknown.IsRunning = activate;
            this.statUnknown.IsVisible = !activate;
        }

        /// <summary>
        /// Perform a full analysis
        /// </summary>
        /// <returns>True if the operation is completed correctly</returns>
        private async Task AnalyzeText()
        {
            if (Regex.Matches(this.ExContentEditor.Text, "\\w+").Cast<Match>().Select(m => m.Value).Count() <= this.ex.MinimumWords * 2 / 3)
            {
                throw new OperationCanceledException("Text is too short.");
            }

            if (this._isAnotherAnalysisRunning)
            {
                throw new OperationCanceledException("Another analysis is running.");
            }

            if (string.IsNullOrWhiteSpace(this.ExContentEditor.Text))
            {
                throw new OperationCanceledException("Another analysis is running.");
            }

            #region Cleaning the input text
            if (this.ExContentEditor.Text.Contains("’"))
            {
                this.ExContentEditor.Text = this.ExContentEditor.Text.Replace("’", "'");
            }

            if (this.ExContentEditor.Text.Contains("`"))
            {
                this.ExContentEditor.Text = this.ExContentEditor.Text.Replace("`", "'");
            }

            if (this.ExContentEditor.Text.Contains("、"))
            {
                this.ExContentEditor.Text = this.ExContentEditor.Text.Replace("、", "'");
            }

            if (this.ExContentEditor.Text.Contains("،"))
            {
                this.ExContentEditor.Text = this.ExContentEditor.Text.Replace("،", "'");
            }

            if (this.ExContentEditor.Text.Contains("‘"))
            {
                this.ExContentEditor.Text = this.ExContentEditor.Text.Replace("‘", "'");
            }

            if (this.ExContentEditor.Text.Contains("“"))
            {
                this.ExContentEditor.Text = this.ExContentEditor.Text.Replace("“", "\"");
            }

            if (this.ExContentEditor.Text.Contains("”"))
            {
                this.ExContentEditor.Text = this.ExContentEditor.Text.Replace("”", "\"");
            }

            Tools.Logger.Log(this, "Replace all the contractions");
            this.ex.Contents = OfflineWord.ReplaceAllEnglishContractions(this.ExContentEditor.Text.ToLower());
            #endregion

            Tools.Logger.Log(this, "Start new offline analysis.");
            this._changeActivityIndicatorsStatus(true);

            try
            {
                await this.ex.PerformFullOfflineAnalysis(true);
                Tools.Logger.Log(this, "Offline analysis done correctly.");

                this.statUnknown.Text = string.Format("{0:P0}", this.ex.GetNumWordsPercentage(new List<PartOfSpeech> { PartOfSpeech.Unclassified }));
                this.statNouns.Text = string.Format("{0:P0}", this.ex.GetNumWordsPercentage(new List<PartOfSpeech> { PartOfSpeech.CommonNoun, PartOfSpeech.ProperNoun }));
                this.statAdverbs.Text = string.Format("{0:P0}", this.ex.GetNumWordsPercentage(new List<PartOfSpeech> { PartOfSpeech.Adverb }));
                this.statAdjective.Text = string.Format("{0:P0}", this.ex.GetNumWordsPercentage(new List<PartOfSpeech> { PartOfSpeech.Adjective }));
                this.statVerbs.Text = string.Format("{0:P0}", this.ex.GetNumWordsPercentage(new List<PartOfSpeech> { PartOfSpeech.Verb, PartOfSpeech.ModalVerb }));

                this._changeActivityIndicatorsStatus(false);

                this._isAnotherAnalysisRunning = false;
            }
            catch (Exception ex)
            {
                Tools.Logger.Log(this, "Offline analysis didn't exit correctly.", ex);

                // TODO: localize!
                bool userChoice = await this.DisplayAlert("Error", "Error while performing the analysis.\nSpecific message:\n" + ex.Message, "Retry", "Cancel the operation");
                if (userChoice)
                {
                    this._isAnotherAnalysisRunning = false;
                    await this.AnalyzeText();
                }
                else
                {
                    this._isAnotherAnalysisRunning = false;
                    this._changeActivityIndicatorsStatus(false);
                }
            }
        }
    }
}