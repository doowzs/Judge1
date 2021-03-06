import { Component, Input } from '@angular/core';

import { SubmissionService } from '../../../../services/submission.service';
import { Program } from '../../../../../interfaces/submission.interfaces';

@Component({
  selector: 'app-submission-ordinary-creator',
  templateUrl: './ordinary.component.html',
  styleUrls: ['./ordinary.component.css']
})
export class SubmissionOrdinaryCreatorComponent {
  @Input() public problemId: number;

  public submitting = false;
  public submissionId: number = undefined;

  constructor(
    private service: SubmissionService,
  ) {
  }

  public submit(program: Program): void {
    this.submitting = true;
    this.service.createSingle(this.problemId, program)
      .subscribe(submission => {
        this.submissionId = submission.id;
        setTimeout(() => {
          this.submitting = false;
          this.submissionId = undefined;
        }, 5000);
      }, error => {
        console.log(error);
        this.submitting = false;
      });
  }
}
