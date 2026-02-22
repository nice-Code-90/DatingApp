import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';

@Pipe({ name: 'chatMarkdown' })
export class ChatMarkdownPipe implements PipeTransform {
  constructor(private sanitizer: DomSanitizer) {}

  transform(value: string) {
    const html = value.replace(
      /\[(.*?)\]\((.*?)\)/g,
      '<a class="link link-primary font-bold" href="$2">$1</a>',
    );
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }
}
