import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';

@Pipe({ name: 'chatMarkdown' })
export class ChatMarkdownPipe implements PipeTransform {
  constructor(private sanitizer: DomSanitizer) {}

  transform(value: string) {
    const html = value.replace(
      /\[(.*?)\]\((.*?)\)/g,
      '<a class="text-sky-200 underline font-bold cursor-pointer transition-colors hover:text-sky-100" href="$2">$1</a>',
    );
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }
}
